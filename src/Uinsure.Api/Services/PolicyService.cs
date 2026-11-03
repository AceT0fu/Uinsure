using Uinsure.Api.DTOs;
using Uinsure.Api.Infrastructure;
using Uinsure.Api.Validation;
using Uinsure.Models;

namespace Uinsure.Api.Services;

public class PolicyService(IPolicyRepository policyRepository, PolicyValidator policyValidator) : IPolicyService
{
    public Task<Policy?> GetPolicyAsync()
    {
        throw new NotImplementedException();
    }

    public async Task<Policy> SellPolicyAsync(SellPolicyRequest request)
    {
        var errors = await policyValidator.ValidateSellPolicy(request);

        if (errors.Any())
        {
            throw new ValidationException(errors);
        }

        // Could be moved to a mapper later to reduce code clutter/duplication at a later stage.
        var policy = new Policy()
        {
            Reference = request.Reference,
            Type = request.Type,
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

    public Task<Policy> RenewPolicyAsync()
    {
        throw new NotImplementedException();
    }

    public Task<Refund> CancelPolicyAsync()
    {
        throw new NotImplementedException();
    }




}
