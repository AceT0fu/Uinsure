using Uinsure.Models.Enums;

namespace Uinsure.Models;

public class Payment
{
    public string PaymentReference { get; set; }
    public PaymentType Type { get; set; }
    public decimal Amount { get; set; }
}
