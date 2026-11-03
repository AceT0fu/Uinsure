using Uinsure.Api.DTOs;
using Uinsure.Models;

namespace Uinsure.Api.Services;

public interface IPolicyService
{
    Task<Policy?> GetPolicyAsync();
    Task<Policy> SellPolicyAsync(SellPolicyRequest request);
    Task<Refund> CancelPolicyAsync();
    Task<Policy> RenewPolicyAsync();
}
