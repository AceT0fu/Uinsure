using Uinsure.Models;
using Uinsure.Models.Enums;

namespace Uinsure.Api.DTOs;

public class SellPolicyRequest
{
    public string Reference { get; set; }
    public PolicyType PolicyType { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal Amount { get; set; }
    public bool AutoRenew { get; set; }
    public List<PolicyHolder> PolicyHolders { get; set; }
    public Property Property { get; set; }
    public PaymentType PaymentType { get; set; }

}
