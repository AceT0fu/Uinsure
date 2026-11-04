using Uinsure.Api.DTOs;
using Uinsure.Models;

namespace Uinsure.Api.Services;

public interface IPolicyService
{
    Task<Policy?> GetPolicyAsync(string reference);
    Task<Policy> SellPolicyAsync(SellPolicyRequest request);
    Task<Refund> CancelPolicyAsync(string reference);
    Task<Policy> RenewPolicyAsync(string reference);
}
