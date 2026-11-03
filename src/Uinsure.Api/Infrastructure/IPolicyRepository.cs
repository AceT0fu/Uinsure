using Uinsure.Models;

namespace Uinsure.Api.Infrastructure;

public interface IPolicyRepository
{
    Task<Policy?> GetPolicyByReferenceAsync(string reference);
    Task CreatePolicyAsync(Policy policy);
    Task<bool> ExistsPolicyAsync(string reference);
    Task UpdatePolicyAsync(Policy policy);
}
