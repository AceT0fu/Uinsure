using Uinsure.Models.Enums;

namespace Uinsure.Models;

public class Refund
{
    public string RefundReference { get; set; }
    public PaymentType Type { get; set; }
    public double Amount { get; set; }
}
