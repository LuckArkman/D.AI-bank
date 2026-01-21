using Fintech.Interfaces;
using Fintech.Entities;
using Fintech.Regulatory;

namespace Fintech.Services;

public class TaxationService : ITaxationService
{
    private readonly IRegulatoryRegistry _registry;
    private readonly ITenantRepository _tenantRepo;
    private readonly ITenantProvider _tenantProvider;

    public TaxationService(
        IRegulatoryRegistry registry,
        ITenantRepository tenantRepo,
        ITenantProvider tenantProvider)
    {
        _registry = registry;
        _tenantRepo = tenantRepo;
        _tenantProvider = tenantProvider;
    }

    public async Task<decimal> CalculateTotalTaxAsync(Account account, decimal amount, string operationType)
    {
        var tenantId = _tenantProvider.TenantId ?? throw new Exception("TenantId context missing");
        var tenant = await _tenantRepo.GetByIdAsync(tenantId);

        if (tenant == null) return 0;

        decimal totalTax = 0;

        foreach (var jurisdiction in tenant.ActiveJurisdictions)
        {
            var pack = _registry.GetPack(jurisdiction);
            totalTax += pack.CalculateTax(amount, operationType);
        }

        return totalTax;
    }
}
