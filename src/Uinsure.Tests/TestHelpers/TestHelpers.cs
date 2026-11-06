using Uinsure.Api.Models.Enums;
using Uinsure.Models;
using Uinsure.Models.Enums;

namespace Uinsure.Tests.TestHelpers;

public static class TestHelpers
{
    public const string TestReference = "TEST001";
    public static DateTime TestStartTime = DateTime.UtcNow;

    public static Policy CreateExistingPolicy(string reference = TestReference)
    {
        var startDate = TestStartTime.AddYears(-1).AddDays(1);

        return new Policy
        {
            Reference = reference,
            PolicyType = PolicyType.BuyToLet,
            StartDate = startDate,
            EndDate = startDate.AddYears(1),
            Amount = 1000m,
            AutoRenew = true,
            HasClaims = false,
            PolicyHolders = new List<PolicyHolder>
            {
                new PolicyHolder
                {
                    FirstName = "John",
                    LastName = "Doe",
                    DateOfBirth = new DateTime(1990, 1, 1)
                }
            },
            Property = new Property
            {
                AddressLine1 = "123 Main St",
                AddressLine2 = "Apt 4B",
                AddressLine3 = "",
                PostCode = "12345"
            },
            Payment = new Payment
            {
                PaymentReference = Guid.NewGuid().ToString(),
                Type = PaymentType.Card,
                Amount = 1000m
            },
            Status = PolicyStatus.Active
        };
    }
}
