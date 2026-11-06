using Uinsure.Api.Infrastructure;
using Uinsure.Api.Models.Enums;
using Uinsure.Api.Validation.Exceptions;
using static Uinsure.Tests.TestHelpers.TestHelpers;

namespace Uinsure.Tests.PolicyRepositoryTests;

public class InMemoryRepositoryTests
{
    [Fact]
    public async Task GetPolicyByReference_WhenPolicyExists_ReturnsPolicy()
    {
        var repository = new InMemoryPolicyRepository();

        var policy = await repository.GetPolicyByReferenceAsync("POL001");

        Assert.NotNull(policy);
        Assert.Equal("POL001", policy.Reference);
    }

    [Fact]
    public async Task GetPolicyByReference_WhenPolicyDoesNotExist_ThrowsNotFoundException()
    {
        var repository = new InMemoryPolicyRepository();

        await Assert.ThrowsAsync<NotFoundException>(
            () => repository.GetPolicyByReferenceAsync("MISSING"));
    }

    [Fact]
    public async Task CreatePolicy_WhenReferenceIsUnique_AddsPolicy()
    {
        var repository = new InMemoryPolicyRepository();

        var reference = "NEW001";
        var policy = CreateExistingPolicy(reference);

        await repository.CreatePolicyAsync(policy);

        var created = await repository.GetPolicyByReferenceAsync(reference);

        Assert.Equal(reference, created.Reference);
    }

    [Fact]
    public async Task CreatePolicy_WhenReferenceAlreadyExists_ThrowsValidationException()
    {
        var repository = new InMemoryPolicyRepository();

        var policy = CreateExistingPolicy("POL001");

        await Assert.ThrowsAsync<ValidationException>(
            () => repository.CreatePolicyAsync(policy));
    }

    [Fact]
    public async Task UpdatePolicy_WhenPolicyExists_UpdatesPolicy()
    {
        var repository = new InMemoryPolicyRepository();

        var policy = await repository.GetPolicyByReferenceAsync("POL001");

        policy.Status = PolicyStatus.Cancelled;
        await repository.UpdatePolicyAsync(policy);

        var updated = await repository.GetPolicyByReferenceAsync("POL001");

        Assert.Equal(
            PolicyStatus.Cancelled,
            updated.Status);
    }

    [Fact]
    public async Task UpdatePolicy_WhenPolicyDoesNotExist_ThrowsNotFoundException()
    {
        var repository = new InMemoryPolicyRepository();

        var policy = CreateExistingPolicy("MISSING");

        await Assert.ThrowsAsync<NotFoundException>(
            () => repository.UpdatePolicyAsync(policy));
    }
}
