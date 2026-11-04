using Uinsure.Api.DTOs;
using Uinsure.Models;
using Uinsure.Api.Models.Enums;
using Uinsure.Api.Validation.Exceptions;

namespace Uinsure.Api.Validation;

// Throws a list of errors from validating the request
public static class PolicyValidator
{

    public static List<string> ValidateSellPolicyRequest(SellPolicyRequest request)
    {
        var errors = new List<string>();

        const int MAX_DAYS_BEFORE_START = 60;
        const int POLICY_DURATION = 1; // years
        const int MIN_POLICY_HOLDERS = 1;
        const int MAX_POLICY_HOLDERS = 3;
        const int MIN_POLICY_HOLDER_AGE = 16; // years

        if (request.StartDate > DateTime.UtcNow.AddDays(MAX_DAYS_BEFORE_START))
        { 
            errors.Add($"Policy start date cannot be more than {MAX_DAYS_BEFORE_START} days in the future.");
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

    public static List<string> ValidateRenewPolicy(Policy policy)
    {
        var errors = new List<string>();

        const int MAX_DAYS_BEFORE_END = 30;

        if (policy.Status != PolicyStatus.Active)
        {
            throw new ValidationException(new List<string> { $"Policy has already been {policy.Status}." });
        }

        if (DateTime.UtcNow < policy.EndDate.AddDays(-MAX_DAYS_BEFORE_END))
        {
            errors.Add($"Policy with cannot be renewed more than {MAX_DAYS_BEFORE_END} days before its end date.");
        }

        if (DateTime.UtcNow > policy.EndDate)
        {
            errors.Add($"Policy with cannot be renewed after its end date.");
        }

        return errors;
    }
}
