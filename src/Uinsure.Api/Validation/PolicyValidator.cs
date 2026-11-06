using Uinsure.Api.DTOs;
using Uinsure.Models;
using Uinsure.Models.Enums;
using static Uinsure.Api.Validation.Constants;

namespace Uinsure.Api.Validation;

// Throws a list of errors from validating the request
public static class PolicyValidator
{
    public static List<string> ValidateSellPolicyRequest(SellPolicyRequest request)
    {
        var errors = new List<string>();

        if (request.StartDate.Date > DateTime.UtcNow.AddDays(SELL_MAX_DAYS_BEFORE_START).Date)
        { 
            errors.Add($"Policy start date cannot be more than {SELL_MAX_DAYS_BEFORE_START} days in the future.");
        }

        if (request.StartDate.AddYears(SELL_POLICY_DURATION).Date != request.EndDate.Date)
        {
            errors.Add($"Policy end date must be exactly {SELL_POLICY_DURATION} years after the start date.");
        }

        if (request.PolicyHolders == null)
        {
            errors.Add("At least one policy holder is required.");
            return errors;
        }

        if (request.PolicyHolders.Count < SELL_MIN_POLICY_HOLDERS || 
            request.PolicyHolders.Count > SELL_MAX_POLICY_HOLDERS)
        {
            errors.Add($"Policy must have between {SELL_MIN_POLICY_HOLDERS} and {SELL_MAX_POLICY_HOLDERS} policy holders.");
        }

        // Requirements says over 16, but I'm assuming that means 16 and over
        if (request.PolicyHolders.Any(ph => ph.DateOfBirth.AddYears(SELL_MIN_POLICY_HOLDER_AGE) > request.StartDate))
        {
            errors.Add($"All policy holders must be at least {SELL_MIN_POLICY_HOLDER_AGE} years old.");
        }

        return errors;
    }

    public static List<string> ValidateRenewPolicy(Policy policy)
    {
        var errors = new List<string>();

        if (DateTime.UtcNow < policy.EndDate.AddDays(-RENEW_MAX_DAYS_BEFORE_END))
        {
            errors.Add($"Policy with cannot be renewed more than {RENEW_MAX_DAYS_BEFORE_END} days before its end date.");
        }

        if (DateTime.UtcNow > policy.EndDate)
        {
            errors.Add($"Policy with cannot be renewed after its end date.");
        }

        if (policy.AutoRenew &&
            policy.Payment.Type == PaymentType.Cheque)
        {
            errors.Add("Auto-renew policy renewals cannot be paid by cheque.");
        }

        return errors;
    }
}
