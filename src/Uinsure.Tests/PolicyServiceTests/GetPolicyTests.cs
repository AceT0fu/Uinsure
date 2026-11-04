using Moq;
using Uinsure.Api.Infrastructure;
using Uinsure.Api.Services;
using Uinsure.Api.Validation.Exceptions;
using Uinsure.Models;

namespace Uinsure.Tests.PolicyServiceTests;

public class GetPolicyTests
{
    [Fact]
    public async Task GetPolicy_WhenPolicyExists_ReturnsPolicy()
    {
        // Arrange
        var reference = "POL001";

        var expectedPolicy = new Policy
        {
            Reference = reference
        };

        var repository = new Mock<IPolicyRepository>();

        repository
            .Setup(r => r.GetPolicyByReferenceAsync(reference))
            .ReturnsAsync(expectedPolicy);

        var service = new PolicyService(repository.Object);

        // Act
        var policy = await service.GetPolicyAsync(reference);

        // Assert
        Assert.NotNull(policy);
        Assert.Equal(reference, policy.Reference);
    }

    [Fact]
    public async Task GetPolicy_WhenPolicyDoesNotExist_ThrowsNotFoundException()
    {
        // Arrange
        var repository = new Mock<IPolicyRepository>();

        repository
            .Setup(r => r.GetPolicyByReferenceAsync("MISSING"))
            .ThrowsAsync(
                new NotFoundException("Policy does not exist."));

        var service = new PolicyService(repository.Object);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(
            () => service.GetPolicyAsync("MISSING"));
    }
}
