using Fintech.Regulatory;
using Microsoft.AspNetCore.Mvc;
using Fintech.Interfaces;
using Fintech.Enums;

namespace Fintech.Controllers;

[ApiController]
[Route("api/v1/tenet/compliance")]
public class ComplianceController : ControllerBase
{
    private readonly IRegulatoryRegistry _registry;
    private readonly ITenantRepository _tenantRepo;
    private readonly ITenantProvider _tenantProvider;

    public ComplianceController(
        IRegulatoryRegistry registry,
        ITenantRepository tenantRepo,
        ITenantProvider tenantProvider)
    {
        _registry = registry;
        _tenantRepo = tenantRepo;
        _tenantProvider = tenantProvider;
    }

    [HttpGet("active-packs")]
    public async Task<IActionResult> GetActivePacks()
    {
        var tenant = await _tenantRepo.GetByIdAsync(_tenantProvider.TenantId ?? Guid.Empty);
        if (tenant == null) return NotFound("Tenant context missing");

        var packs = tenant.ActiveJurisdictions.Select(j => new
        {
            Jurisdiction = j.ToString(),
            Details = "Regulatory Compliance Pack Loaded"
        });

        return Ok(new
        {
            Tenant = tenant.Name,
            ActivePacks = packs
        });
    }

    [HttpPost("activate-jurisdiction/{jurisdiction}")]
    public async Task<IActionResult> ActivateJurisdiction(Jurisdiction jurisdiction)
    {
        var tenant = await _tenantRepo.GetByIdAsync(_tenantProvider.TenantId ?? Guid.Empty);
        if (tenant == null) return NotFound();

        tenant.AddJurisdiction(jurisdiction);
        await _tenantRepo.UpdateAsync(tenant);

        return Ok(new { Message = $"{jurisdiction} activated for {tenant.Name}" });
    }
}
