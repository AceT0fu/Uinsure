using Uinsure.Api.DTOs;
using Uinsure.Api.Infrastructure;
using Uinsure.Api.Models.Enums;
using Uinsure.Api.Validation;
using Uinsure.Api.Validation.Exceptions;
using Uinsure.Models;
using static Uinsure.Api.Validation.Constants;

namespace Uinsure.Api.Services;

public class PolicyService(IPolicyRepository policyRepository) : IPolicyService
{
    public async Task<Policy?> GetPolicyAsync(string reference)
    {
        return await policyRepository.GetPolicyByReferenceAsync(reference);
    }

    public async Task<Policy> SellPolicyAsync(SellPolicyRequest request)
    {
        var errors = PolicyValidator.ValidateSellPolicyRequest(request);

        if (errors.Any())
        {
            throw new ValidationException(errors);
        }

        // Could be moved to a mapper later to reduce code clutter/duplication.
        var policy = new Policy()
        {
            Reference = request.Reference,
            PolicyType = request.PolicyType,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            Amount = request.Amount,
            HasClaims = false,
            AutoRenew = request.AutoRenew,
            PolicyHolders = request.PolicyHolders.Select(ph => new PolicyHolder
            {
                FirstName = ph.FirstName,
                LastName = ph.LastName,
                DateOfBirth = ph.DateOfBirth
            })
            .ToList(),
            Property = new Property
            {
                AddressLine1 = request.Property.AddressLine1,
                AddressLine2 = request.Property.AddressLine2,
                AddressLine3 = request.Property.AddressLine3,
                PostCode = request.Property.PostCode
            },
            Payment = new Payment()
            {
                PaymentReference = Guid.NewGuid().ToString(),
                Type = request.PaymentType,
                Amount = request.Amount,
            }
        };

        await policyRepository.CreatePolicyAsync(policy);

        return policy;
    }

    public async Task<Policy> RenewPolicyAsync(string reference)
    {
        var policy = await GetActivePolicyAsync(reference);

        var errors = PolicyValidator.ValidateRenewPolicy(policy);

        if (errors.Any())
        {
            throw new ValidationException(errors);
        }

        var renewedPolicy = new Policy()
        {
            Reference = Guid.NewGuid().ToString(),
            PreviousReference = policy!.Reference,
            PolicyType = policy.PolicyType,
            StartDate = policy.EndDate,
            EndDate = policy.EndDate.AddYears(1),
            Amount = policy.Amount,
            HasClaims = false,
            AutoRenew = policy.AutoRenew,
            PolicyHolders = policy.PolicyHolders.Select(ph => new PolicyHolder
            {
                FirstName = ph.FirstName,
                LastName = ph.LastName,
                DateOfBirth = ph.DateOfBirth
            })
            .ToList(),
            Property = new Property
            {
                AddressLine1 = policy.Property.AddressLine1,
                AddressLine2 = policy.Property.AddressLine2,
                AddressLine3 = policy.Property.AddressLine3,
                PostCode = policy.Property.PostCode
            }
        };

        if (policy.AutoRenew)
        {
            renewedPolicy.Payment = new Payment
            {
                PaymentReference = Guid.NewGuid().ToString(),
                Type = policy.Payment.Type,
                Amount = policy.Amount,
            };
        }

        // For renewals, create a new policy to keep the history of the previous policy intact
        await policyRepository.CreatePolicyAsync(renewedPolicy);

        return renewedPolicy;
    }

    public async Task<Refund> GetCancellationQuoteAsync(string reference)
    {
        var policy = await GetActivePolicyAsync(reference);

        return new Refund
        {
            Reference = policy.Reference,
            Type = policy.Payment.Type,
            Amount = CalculateRefundAmount(policy)
        };
    }

    public async Task<Refund> CancelPolicyAsync(string reference)
    {
        var policy = await GetActivePolicyAsync(reference);

        policy.Status = PolicyStatus.Cancelled;
        await policyRepository.UpdatePolicyAsync(policy);

        return new Refund
        {
            Reference = policy.Reference,
            Type = policy.Payment.Type,
            Amount = CalculateRefundAmount(policy)
        };
    }

    private async Task<Policy> GetActivePolicyAsync(string reference)
    {
        var policy = await policyRepository.GetPolicyByReferenceAsync(reference);

        if (policy.Status != PolicyStatus.Active)
        {
            throw new ValidationException(
            [
                $"Policy has already been {policy.Status}."
            ]);
        }

        return policy;
    }

    private decimal CalculateRefundAmount(Policy policy)
    {
        if (DateTime.UtcNow <= policy.StartDate.AddDays(CANCEL_COOL_OFF_DAYS))
        {
            return policy.Amount;
        }

        if (DateTime.UtcNow >= policy.EndDate)
        {
            return 0;
        }

        if (policy.HasClaims)
        {
            return 0;
        }

        // Calculate pro rata refund based on the remaining days of the policy
        var daysRemaining = (policy.EndDate - DateTime.UtcNow).TotalDays;
        var policyDays = (policy.EndDate - policy.StartDate).TotalDays;

        return Math.Round(
            policy.Amount * 
            (decimal)(daysRemaining / policyDays), 
            2);
        
    }
}
