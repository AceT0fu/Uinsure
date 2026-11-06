using Moq;
using Uinsure.Api.DTOs;
using Uinsure.Api.Infrastructure;
using Uinsure.Api.Services;
using Uinsure.Api.Validation;
using Uinsure.Api.Validation.Exceptions;
using Uinsure.Models;
using Uinsure.Models.Enums;
using static Uinsure.Tests.TestHelpers.TestHelpers;
using static Uinsure.Api.Validation.Constants;

namespace Uinsure.Tests.PolicyServiceTests;

public class SellPolicyTests
{
    private static SellPolicyRequest CreateValidSellPolicyRequest()
    {
        var startDate = TestStartTime;

        return new SellPolicyRequest
        {
            Reference = "TEST001",
            PolicyType = PolicyType.BuyToLet,
            StartDate = startDate,
            EndDate = startDate.AddYears(1),
            Amount = 1000m,
            AutoRenew = true,
            PolicyHolders = new List<PolicyHolder>
            {
                new PolicyHolder
                {
                    FirstName = "John",
                    LastName = "Doe",
                    DateOfBirth = new DateTime(1990, 1, 1)
                }
            },
            Property = new Property
            {
                AddressLine1 = "123 Main St",
                AddressLine2 = "Apt 4B",
                AddressLine3 = "",
                PostCode = "12345"
            },
            PaymentType = PaymentType.Card
        };
    }

    [Fact]
    public async Task SellPolicy_WhenRequestIsValid_CreatesPolicy()
    {
        // Arrange
        var repository = new Mock<IPolicyRepository>();
        var service = new PolicyService(repository.Object);

        var request = CreateValidSellPolicyRequest();

        // Act
        var policy = await service.SellPolicyAsync(request);

        // Assert
        Assert.NotNull(policy);

        Assert.Equal(request.Reference, policy.Reference);
        Assert.Equal(request.PolicyType, policy.PolicyType);
        Assert.Equal(request.Amount, policy.Amount);
        Assert.Equal(request.AutoRenew, policy.AutoRenew);

        Assert.False(policy.HasClaims);

        Assert.NotNull(policy.Payment);
        Assert.Equal(request.PaymentType, policy.Payment.Type);
        Assert.Equal(request.Amount, policy.Payment.Amount);
        Assert.False(string.IsNullOrWhiteSpace(
            policy.Payment.PaymentReference));

        repository.Verify(
            r => r.CreatePolicyAsync(
                It.Is<Policy>(p =>
                    p.Reference == request.Reference &&
                    p.Amount == request.Amount &&
                    p.Payment != null)),
            Times.Once);
    }

    [Fact]
    public async Task SellPolicy_WhenPolicyStartsTooLate_ThrowsValidationException()
    {
        // Arrange
        var repository = new Mock<IPolicyRepository>();
        var service = new PolicyService(repository.Object);

        var request = CreateValidSellPolicyRequest();
        request.StartDate = TestStartTime.AddDays(SELL_MAX_DAYS_BEFORE_START + 1);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(
            () => service.SellPolicyAsync(request));

        Assert.Contains(
            $"Policy start date cannot be more than {SELL_MAX_DAYS_BEFORE_START} days in the future.",
            exception.Errors);
    }

    [Fact]
    public async Task SellPolicy_WhenReferenceAlreadyExists_ThrowsValidationException()
    {
        // Arrange
        var request = CreateValidSellPolicyRequest();

        var repository = new Mock<IPolicyRepository>();

        repository
            .Setup(r => r.CreatePolicyAsync(It.IsAny<Policy>()))
            .ThrowsAsync(new ValidationException(new[]
            {
            $"Policy with reference {request.Reference} already exists."
            }));

        var service = new PolicyService(repository.Object);

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(
            () => service.SellPolicyAsync(request));
    }

    [Fact]
    public async Task SellPolicy_WhenPolicyDurationIsInvalid_ThrowsValidationException()
    {
        // Arrange
        var repository = new Mock<IPolicyRepository>();
        var service = new PolicyService(repository.Object);

        var request = CreateValidSellPolicyRequest();
        request.EndDate = request.StartDate.AddYears(SELL_POLICY_DURATION + 1);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(
            () => service.SellPolicyAsync(request));

        Assert.Contains(
            $"Policy end date must be exactly {SELL_POLICY_DURATION} years after the start date.",
            exception.Errors);
    }

    [Fact]
    public async Task SellPolicy_WhenNoPolicyHolders_ThrowsValidationException()
    {
        // Arrange
        var repository = new Mock<IPolicyRepository>();
        var service = new PolicyService(repository.Object);

        var request = CreateValidSellPolicyRequest();
        request.PolicyHolders = new List<PolicyHolder>();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(
            () => service.SellPolicyAsync(request));

        Assert.Contains(
            $"Policy must have between {SELL_MIN_POLICY_HOLDERS} and {SELL_MAX_POLICY_HOLDERS} policy holders.",
            exception.Errors);
    }

    [Fact]
    public async Task SellPolicy_WhenTooManyPolicyHolders_ThrowsValidationException()
    {
        // Arrange
        var repository = new Mock<IPolicyRepository>();
        var service = new PolicyService(repository.Object);

        var request = CreateValidSellPolicyRequest();
        for (int i = 0; i < SELL_MAX_POLICY_HOLDERS; i++)
        {
            request.PolicyHolders.Add(new PolicyHolder
            {
                FirstName = $"John{i}",
                LastName = $"Doe{i}",
                DateOfBirth = new DateTime(1990, 1, 1)
            });
        }

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(
            () => service.SellPolicyAsync(request));

        Assert.Contains(
            $"Policy must have between {SELL_MIN_POLICY_HOLDERS} and {SELL_MAX_POLICY_HOLDERS} policy holders.",
            exception.Errors);
    }

    [Fact]
    public async Task SellPolicy_WhenAPolicyHolderIsTooYoung_ThrowsValidationException()
    {
        // Arrange
        var repository = new Mock<IPolicyRepository>();
        var service = new PolicyService(repository.Object);

        var request = CreateValidSellPolicyRequest();

        var birthYear = request.StartDate.AddYears(-SELL_MIN_POLICY_HOLDER_AGE + 1).Year;
        request.PolicyHolders.Add(new PolicyHolder
        {
            FirstName = $"John",
            LastName = $"Doe",
            DateOfBirth = new DateTime(birthYear, 1, 1)
        });

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(
            () => service.SellPolicyAsync(request));

        Assert.Contains(
            $"All policy holders must be at least {SELL_MIN_POLICY_HOLDER_AGE} years old.",
            exception.Errors);
    }

    [Fact]
    public async Task SellPolicy_WhenPolicyHolderBecomesMinimumAgeOnPolicyStart_CreatesPolicy()
    {
        // Arrange
        var repository = new Mock<IPolicyRepository>();
        var service = new PolicyService(repository.Object);

        var request = CreateValidSellPolicyRequest();
        request.StartDate = TestStartTime.AddDays(1);
        request.EndDate = TestStartTime.AddDays(366);
        
        request.PolicyHolders.Add(new PolicyHolder
        {
            FirstName = $"John",
            LastName = $"Doe",
            DateOfBirth = request.StartDate.AddYears(-SELL_MIN_POLICY_HOLDER_AGE)
        });

        // Act
        var policy = await service.SellPolicyAsync(request);

        // Assert
        Assert.NotNull(policy);
    }
}
