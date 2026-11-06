using Uinsure.Api.Models.Enums;
using Uinsure.Models.Enums;

namespace Uinsure.Models;

public class Policy
{
    public string Reference { get; set; }
    public string PreviousReference { get; set; } // old reference before renewal
    public PolicyType PolicyType { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal Amount { get; set; }
    public bool HasClaims { get; set; }
    public bool AutoRenew { get; set; }
    public List<PolicyHolder> PolicyHolders { get; set; }
    public Property Property { get; set; }
    public Payment Payment { get; set; }
    public PolicyStatus Status { get; set; } = PolicyStatus.Active;
}
