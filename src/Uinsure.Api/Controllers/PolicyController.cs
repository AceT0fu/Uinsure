using Microsoft.AspNetCore.Mvc;
using Uinsure.Api.DTOs;
using Uinsure.Api.Services;
using Uinsure.Models;

namespace Uinsure.Controllers;

[ApiController]
[Route("api/policies")]
public class PoliciesController : ControllerBase
{
    private readonly PolicyService _policyService;

    public PoliciesController(PolicyService policyService)
    {
        _policyService = policyService;
    }

    [HttpPost]
    public async Task<ActionResult<Policy>> SellPolicy(SellPolicyRequest request)
    {
        var policy = await _policyService.SellPolicyAsync(request);

        return CreatedAtAction(
            nameof(GetPolicy),
            new { reference = policy.Reference },
            policy);
    }

    [HttpGet("{reference}")]
    public async Task<ActionResult<Policy>> GetPolicy(string reference)
    {
        var policy = await _policyService.GetPolicyAsync(reference);

        if (policy == null)
        {
            return NotFound();
        }

        return Ok(policy);
    }

    [HttpPost("{reference}/renew")]
    public async Task<ActionResult<Policy>> RenewPolicy(string reference)
    {
        var renewedPolicy = await _policyService.RenewPolicyAsync(reference);

        return Ok(renewedPolicy);
    }

    [HttpPost("{reference}/cancel")]
    public async Task<ActionResult<Refund>> CancelPolicy(string reference)
    {
        var refund = await _policyService.CancelPolicyAsync(reference);

        return Ok(refund);
    }
}
