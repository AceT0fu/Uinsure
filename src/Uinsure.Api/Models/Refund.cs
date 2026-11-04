using Uinsure.Models.Enums;

namespace Uinsure.Models;

public class Refund
{
    public string Reference { get; set; }
    public PaymentType Type { get; set; }
    public decimal Amount { get; set; }
}
