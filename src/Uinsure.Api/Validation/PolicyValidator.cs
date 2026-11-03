using Uinsure.Api.DTOs;
using Uinsure.Api.Infrastructure;

namespace Uinsure.Api.Validation;

// Throws a list of errors from validating the request
public class PolicyValidator
{
    private readonly IPolicyRepository _policyRepository;

    public PolicyValidator(IPolicyRepository policyRepository)
    {
        _policyRepository = policyRepository;
    }

    public async Task<List<string>> ValidateSellPolicy(SellPolicyRequest request)
    {
        var errors = new List<string>();

        const int MAX_ADVANCEMENT_PERIOD = 60; // days
        const int POLICY_DURATION = 1; // years
        const int MIN_POLICY_HOLDERS = 1;
        const int MAX_POLICY_HOLDERS = 3;
        const int MIN_POLICY_HOLDER_AGE = 16; // years

        if (await _policyRepository.ExistsPolicyAsync(request.Reference))
        {
            errors.Add($"Policy with reference {request.Reference} already exists.");
        }

        if (request.StartDate > DateTime.UtcNow.AddDays(MAX_ADVANCEMENT_PERIOD))
        { 
            errors.Add($"Policy start date cannot be more than {MAX_ADVANCEMENT_PERIOD} days in the future.");
        }

        if (request.StartDate.AddYears(POLICY_DURATION) != request.EndDate)
        {
            errors.Add($"Policy end date must be exactly {POLICY_DURATION} years after the start date.");
        }

        if (request.PolicyHolders == null)
        {
            errors.Add("At least one policy holder is required.");
            return errors;
        }

        if (request.PolicyHolders.Count < MIN_POLICY_HOLDERS || 
            request.PolicyHolders.Count > MAX_POLICY_HOLDERS)
        {
            errors.Add($"Policy must have between {MIN_POLICY_HOLDERS} and {MAX_POLICY_HOLDERS} policy holders.");
        }

        // Requirements says over 16, but I'm assuming that means 16 and over
        if (request.PolicyHolders.Any(ph => ph.DateOfBirth.AddYears(MIN_POLICY_HOLDER_AGE) > request.StartDate))
        {
            errors.Add($"All policy holders must be at least {MIN_POLICY_HOLDER_AGE} years old.");
        }

        return errors;
    }
}
