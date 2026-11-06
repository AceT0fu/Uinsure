using System.Collections.Concurrent;
using Uinsure.Api.Models.Enums;
using Uinsure.Api.Validation.Exceptions;
using Uinsure.Models;
using Uinsure.Models.Enums;

namespace Uinsure.Api.Infrastructure;

public class InMemoryPolicyRepository : IPolicyRepository
{
    private readonly ConcurrentDictionary<string, Policy> _policies = new();

    public Task<Policy> GetPolicyByReferenceAsync(string reference)
    {
        if (!_policies.TryGetValue(reference, out var policy))
        {
            throw new NotFoundException($"Policy with reference {reference} does not exist.");
        }

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

    public Task UpdatePolicyAsync(Policy policy)
    {
        if (!_policies.ContainsKey(policy.Reference))
        {
            throw new NotFoundException($"Policy with reference {policy.Reference} does not exist.");
        }

        _policies[policy.Reference] = policy;
        return Task.CompletedTask;
    }

    public InMemoryPolicyRepository()
    {
        var policy1 = new Policy
        {
            Reference = "POL001",
            PolicyType = PolicyType.Household,
            StartDate = DateTime.UtcNow.AddMonths(-2),
            EndDate = DateTime.UtcNow.AddMonths(10),
            Amount = 500m,
            HasClaims = false,
            AutoRenew = true,
            Status = PolicyStatus.Active,

            PolicyHolders =
            [
                new PolicyHolder
            {
                FirstName = "John",
                LastName = "Smith",
                DateOfBirth = new DateTime(1985, 6, 15)
            }
            ],

            Property = new Property
            {
                AddressLine1 = "1 High Street",
                PostCode = "M1 1AA"
            },

            Payment = new Payment
            {
                PaymentReference = "PAY001",
                Type = PaymentType.Card,
                Amount = 500m
            }
        };

        var policy2 = new Policy
        {
            Reference = "POL002",
            PolicyType = PolicyType.BuyToLet,
            StartDate = DateTime.UtcNow.AddDays(-5),
            EndDate = DateTime.UtcNow.AddYears(1).AddDays(-5),
            Amount = 750m,
            HasClaims = false,
            AutoRenew = false,
            Status = PolicyStatus.Active,

            PolicyHolders =
            [
                new PolicyHolder
            {
                FirstName = "Jane",
                LastName = "Taylor",
                DateOfBirth = new DateTime(1978, 3, 20)
            }
            ],

            Property = new Property
            {
                AddressLine1 = "10 Market Road",
                PostCode = "M2 2BB"
            },

            Payment = new Payment
            {
                PaymentReference = "PAY002",
                Type = PaymentType.DirectDebit,
                Amount = 750m
            }
        };

        _policies.TryAdd(policy1.Reference, policy1);
        _policies.TryAdd(policy2.Reference, policy2);
    }
}
