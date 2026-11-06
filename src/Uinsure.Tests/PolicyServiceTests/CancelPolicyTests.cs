using Moq;
using Uinsure.Api.Infrastructure;
using Uinsure.Api.Models.Enums;
using Uinsure.Api.Services;
using Uinsure.Models;
using static Uinsure.Tests.TestHelpers.TestHelpers;
using static Uinsure.Api.Validation.Constants;
using Uinsure.Api.Validation.Exceptions;

namespace Uinsure.Tests.PolicyServiceTests;

public class CancelPolicyTests
{
    [Fact]
    public async Task CancelPolicy_WhenPolicyIsActive_CancelsPolicyAndCreatesRefund()
    {
        // Arrange
        var existingPolicy = CreateExistingPolicy();

        var repository = new Mock<IPolicyRepository>();
        repository
            .Setup(r => r.GetPolicyByReferenceAsync(TestReference))
            .ReturnsAsync(existingPolicy);

        var service = new PolicyService(repository.Object);

        // Act
        var refund = await service.CancelPolicyAsync(TestReference);

        // Assert
        Assert.NotNull(refund);

        Assert.Equal(existingPolicy.Reference, refund.Reference);
        Assert.Equal(existingPolicy.Payment.Type, refund.Type);
        Assert.True(refund.Amount > 0);

        Assert.Equal(PolicyStatus.Cancelled, existingPolicy.Status);

        repository.Verify(
            r => r.UpdatePolicyAsync(
                It.Is<Policy>(
                    p => p.Status == PolicyStatus.Cancelled)),
            Times.Once);
    }

    [Fact]
    public async Task GetCancellationQuote_WhenPolicyIsActive_ReturnsRefundWithoutUpdatingPolicy()
    {
        // Arrange
        var existingPolicy = CreateExistingPolicy();

        var repository = new Mock<IPolicyRepository>();
        repository
            .Setup(r => r.GetPolicyByReferenceAsync(TestReference))
            .ReturnsAsync(existingPolicy);

        var service = new PolicyService(repository.Object);

        // Act
        var refund = await service.GetCancellationQuoteAsync(TestReference);

        // Assert
        Assert.NotNull(refund);

        Assert.Equal(existingPolicy.Reference, refund.Reference);
        Assert.Equal(existingPolicy.Payment.Type, refund.Type);
        Assert.True(refund.Amount > 0);

        Assert.Equal(PolicyStatus.Active, existingPolicy.Status);

        // No update should occur
        repository.Verify(
            r => r.UpdatePolicyAsync(
                It.IsAny<Policy>()),
            Times.Never);
    }

    [Fact]
    public async Task CancelPolicy_WhenBeforePolicyStart_ReturnsFullRefund()
    {
        // Arrange
        var existingPolicy = CreateExistingPolicy();
        existingPolicy.StartDate = TestStartTime.AddDays(1);
        existingPolicy.EndDate = existingPolicy.StartDate.AddYears(1);

        var repository = new Mock<IPolicyRepository>();
        repository
            .Setup(r => r.GetPolicyByReferenceAsync(TestReference))
            .ReturnsAsync(existingPolicy);

        var service = new PolicyService(repository.Object);

        // Act
        var refund = await service.GetCancellationQuoteAsync(TestReference);

        // Assert
        Assert.NotNull(refund);
        Assert.Equal(existingPolicy.Amount, refund.Amount);
    }

    [Fact]
    public async Task CancelPolicy_WhenWithinCoolingPeriod_ReturnsFullRefund()
    {
        // Arrange
        var existingPolicy = CreateExistingPolicy();
        existingPolicy.StartDate = TestStartTime.AddDays(-CANCEL_COOL_OFF_DAYS + 1);
        existingPolicy.EndDate = existingPolicy.StartDate.AddYears(1);

        var repository = new Mock<IPolicyRepository>();
        repository
            .Setup(r => r.GetPolicyByReferenceAsync(TestReference))
            .ReturnsAsync(existingPolicy);

        var service = new PolicyService(repository.Object);

        // Act
        var refund = await service.GetCancellationQuoteAsync(TestReference);

        // Assert
        Assert.NotNull(refund);
        Assert.Equal(existingPolicy.Amount, refund.Amount);
    }

    [Fact]
    public async Task CancelPolicy_WhenOutsideCoolingOffPeriod_ReturnsProRataRefund()
    {
        // Arrange
        var existingPolicy = CreateExistingPolicy();

        // Policy started 100 days ago
        existingPolicy.StartDate = TestStartTime.AddDays(-100);
        existingPolicy.EndDate = existingPolicy.StartDate.AddYears(1);

        var repository = new Mock<IPolicyRepository>();

        repository
            .Setup(r => r.GetPolicyByReferenceAsync(TestReference))
            .ReturnsAsync(existingPolicy);

        var service = new PolicyService(repository.Object);

        // Calculate expected refund
        var daysRemaining = (existingPolicy.EndDate - DateTime.UtcNow).TotalDays;
        var policyDays = (existingPolicy.EndDate - existingPolicy.StartDate).TotalDays;

        var expectedRefund = Math.Round(
            existingPolicy.Amount *
            (decimal)(daysRemaining / policyDays), 
            2);

        // Act
        var refund =
            await service.GetCancellationQuoteAsync(TestReference);

        // Assert
        Assert.NotNull(refund);
        Assert.Equal(expectedRefund, refund.Amount);
    }

    [Fact]
    public async Task CancelPolicy_WhenHasClaims_ReturnsNoRefund()
    {
        // Arrange
        var existingPolicy = CreateExistingPolicy();
        existingPolicy.HasClaims = true;

        var repository = new Mock<IPolicyRepository>();
        repository
            .Setup(r => r.GetPolicyByReferenceAsync(TestReference))
            .ReturnsAsync(existingPolicy);

        var service = new PolicyService(repository.Object);

        // Act
        var refund = await service.GetCancellationQuoteAsync(TestReference);

        // Assert
        Assert.NotNull(refund);
        Assert.Equal(0, refund.Amount);
    }

    [Fact]
    public async Task CancelPolicy_WhenPolicyAlreadyCancelled_ThrowsValidationException()
    {
        // Arrange
        var existingPolicy = CreateExistingPolicy();
        existingPolicy.Status = PolicyStatus.Cancelled;

        var repository = new Mock<IPolicyRepository>();

        repository
            .Setup(r => r.GetPolicyByReferenceAsync(TestReference))
            .ReturnsAsync(existingPolicy);

        var service = new PolicyService(repository.Object);

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(
            () => service.CancelPolicyAsync(TestReference));
    }
}
