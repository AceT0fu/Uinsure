using System.Collections.Concurrent;
using Uinsure.Models;
using Uinsure.Api.Validation;

namespace Uinsure.Api.Infrastructure;

public class InMemoryPolicyRepository : IPolicyRepository
{
    public ConcurrentDictionary<string, Policy> _policies = new ConcurrentDictionary<string, Policy>();

    public Task<Policy?> GetPolicyByReferenceAsync(string reference)
    {
        _policies.TryGetValue(reference, out var policy);

        return Task.FromResult(policy);
    }

    public Task CreatePolicyAsync(Policy policy)
    {
        if (!_policies.TryAdd(policy.Reference, policy))
        {
            throw new ValidationException([$"Policy with reference {policy.Reference} already exists."]);
        }

        return Task.CompletedTask;
    }

    public Task<bool> ExistsPolicyAsync(string reference)
    {
        var exists = _policies.ContainsKey(reference);

        return Task.FromResult(exists);
    }

    public Task UpdatePolicyAsync(Policy policy)
    {
        if (!_policies.ContainsKey(policy.Reference))
        {
            throw new ValidationException([$"Policy with reference {policy.Reference} does not exist."]);
        }

        _policies[policy.Reference] = policy;
        return Task.CompletedTask;
    }
}
