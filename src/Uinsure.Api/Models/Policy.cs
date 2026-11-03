using Uinsure.Models.Enums;

namespace Uinsure.Models;

public class Policy
{
    public string Reference { get; set; }
    public PolicyType Type { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public double Amount { get; set; }
    public bool HasClaims { get; set; }
    public bool AutoRenew { get; set; }
    public List<PolicyHolder> PolicyHolders { get; set; }
    public Property Property { get; set; }
    public Payment? Payment { get; set; }

}
