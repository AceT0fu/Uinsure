using Uinsure.Models;

namespace Uinsure.Api.Infrastructure;

public interface IPolicyRepository
{
    Task<Policy> GetPolicyByReferenceAsync(string reference);
    Task CreatePolicyAsync(Policy policy);
    Task UpdatePolicyAsync(Policy policy);
}
