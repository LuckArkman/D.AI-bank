using Fintech.Entities;
using Fintech.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Fintech.Controllers;

[ApiController]
[Route("api/v1/tenants")]
public class TenantsController : ControllerBase
{
    private readonly ITenantRepository _tenantRepo;

    public TenantsController(ITenantRepository tenantRepo)
    {
        _tenantRepo = tenantRepo;
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateTenantRequest request)
    {
        var branding = new TenantBranding
        {
            PrimaryColor = request.PrimaryColor ?? "#8B5CF6",
            LogoUrl = request.LogoUrl ?? "",
            CustomDomain = request.CustomDomain ?? ""
        };

        var tenant = new Tenant(request.Name, request.Identifier, branding);
        await _tenantRepo.AddAsync(tenant);

        return Ok(tenant);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var tenants = await _tenantRepo.GetAllAsync();
        return Ok(tenants);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var tenant = await _tenantRepo.GetByIdAsync(id);
        if (tenant == null) return NotFound();
        return Ok(tenant);
    }

    [HttpGet("identifier/{identifier}")]
    public async Task<IActionResult> GetByIdentifier(string identifier)
    {
        var tenant = await _tenantRepo.GetByIdentifierAsync(identifier);
        if (tenant == null) return NotFound();
        return Ok(tenant);
    }
}

public record CreateTenantRequest(string Name, string Identifier, string? PrimaryColor, string? LogoUrl, string? CustomDomain);
