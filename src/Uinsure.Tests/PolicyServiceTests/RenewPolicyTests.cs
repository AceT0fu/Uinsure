using Moq;
using Uinsure.Api.Infrastructure;
using Uinsure.Api.Models.Enums;
using Uinsure.Api.Services;
using Uinsure.Api.Validation.Exceptions;
using Uinsure.Models;
using static Uinsure.Tests.TestHelpers.TestHelpers;
using static Uinsure.Api.Validation.Constants;

namespace Uinsure.Tests.PolicyServiceTests;

public class RenewPolicyTests
{
    [Fact]
    public async Task RenewPolicy_WhenPolicyExistsAndIsValid_RenewsPolicy()
    {
        // Arrange
        var existingPolicy = CreateExistingPolicy();

        var repository = new Mock<IPolicyRepository>();
        repository
            .Setup(r => r.GetPolicyByReferenceAsync(TestReference))
            .ReturnsAsync(existingPolicy);

        var service = new PolicyService(repository.Object);

        // Act
        var renewedPolicy = await service.RenewPolicyAsync(TestReference);

        // Assert
        Assert.NotNull(renewedPolicy);
        Assert.NotEqual(existingPolicy.Reference, renewedPolicy.Reference);
        Assert.Equal(existingPolicy.Reference, renewedPolicy.PreviousReference);
        Assert.Equal(existingPolicy.PolicyType, renewedPolicy.PolicyType);
        Assert.Equal(existingPolicy.Amount, renewedPolicy.Amount);
        Assert.Equal(PolicyStatus.Active, renewedPolicy.Status);

        Assert.Equal(existingPolicy.EndDate, renewedPolicy.StartDate);
        Assert.Equal(existingPolicy.EndDate.AddYears(1), renewedPolicy.EndDate);

        repository.Verify(
            r => r.CreatePolicyAsync(
                It.IsAny<Policy>()),
            Times.Once);
    }


    [Fact]
    public async Task RenewPolicy_WhenAutoRenewTrue_CreatesNewPayment()
    {
        // Arrange
        var existingPolicy = CreateExistingPolicy();
        existingPolicy.AutoRenew = true;

        var repository = new Mock<IPolicyRepository>();
        repository
            .Setup(r => r.GetPolicyByReferenceAsync(TestReference))
            .ReturnsAsync(existingPolicy);

        var service = new PolicyService(repository.Object);

        var oldPaymentReference = existingPolicy.Payment.PaymentReference;

        // Act
        var renewedPolicy = await service.RenewPolicyAsync(TestReference);

        // Assert
        Assert.NotNull(renewedPolicy);
        Assert.NotNull(renewedPolicy.Payment);
        Assert.NotEqual(oldPaymentReference, renewedPolicy.Payment.PaymentReference);
    }

    [Fact]
    public async Task RenewPolicy_WhenAutoRenewFalse_DoesNotCreateNewPayment()
    {
        // Arrange
        var existingPolicy = CreateExistingPolicy();
        existingPolicy.AutoRenew = false;

        var repository = new Mock<IPolicyRepository>();
        repository
            .Setup(r => r.GetPolicyByReferenceAsync(TestReference))
            .ReturnsAsync(existingPolicy);

        var service = new PolicyService(repository.Object);

        // Act
        var renewedPolicy = await service.RenewPolicyAsync(TestReference);

        // Assert
        Assert.NotNull(renewedPolicy);
        Assert.Null(renewedPolicy.Payment);
    }

    [Fact]
    public async Task RenewPolicy_WhenRenewalIsTooEarly_ThrowsValidationException()
    {
        // Arrange
        var existingPolicy = CreateExistingPolicy();
        existingPolicy.EndDate = TestStartTime.AddDays(RENEW_MAX_DAYS_BEFORE_END + 1);
        existingPolicy.StartDate = existingPolicy.EndDate.AddYears(-1);

        var repository = new Mock<IPolicyRepository>();
        repository
            .Setup(r => r.GetPolicyByReferenceAsync(TestReference))
            .ReturnsAsync(existingPolicy);

        var service = new PolicyService(repository.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(
            () => service.RenewPolicyAsync(TestReference));

        Assert.Contains(
            $"Policy with cannot be renewed more than {RENEW_MAX_DAYS_BEFORE_END} days before its end date.", 
            exception.Errors);
    }

    [Fact]
    public async Task RenewPolicy_WhenAfterEndDate_ThrowsValidationException()
    {
        // Arrange
        var existingPolicy = CreateExistingPolicy();
        existingPolicy.EndDate = TestStartTime.AddDays(-1);
        existingPolicy.StartDate = existingPolicy.EndDate.AddYears(-1);

        var repository = new Mock<IPolicyRepository>();
        repository
            .Setup(r => r.GetPolicyByReferenceAsync(TestReference))
            .ReturnsAsync(existingPolicy);

        var service = new PolicyService(repository.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(
            () => service.RenewPolicyAsync(TestReference));

        Assert.Contains(
            $"Policy with cannot be renewed after its end date.",
            exception.Errors);
    }

    [Fact]
    public async Task RenewPolicy_WhenPolicyAlreadyRenewed_ThrowsValidationException()
    {
        // Arrange
        var existingPolicy = CreateExistingPolicy();
        existingPolicy.Status = PolicyStatus.Renewed;

        var repository = new Mock<IPolicyRepository>();
        repository
            .Setup(r => r.GetPolicyByReferenceAsync(TestReference))
            .ReturnsAsync(existingPolicy);

        var service = new PolicyService(repository.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(
            () => service.RenewPolicyAsync(TestReference));
        Assert.Contains(
            $"Policy has already been {existingPolicy.Status}.",
            exception.Errors);
    }
}
