using Fintech.Interfaces;
using Fintech.Entities;
using Fintech.Enums;

namespace Fintech.Regulatory;

public interface IRegulatoryService
{
    Task ValidateTransactionAsync(Account account, decimal amount, string operationType);
}

public class RegulatoryService : IRegulatoryService
{
    private readonly IRegulatoryRegistry _registry;
    private readonly ITenantRepository _tenantRepo;
    private readonly ITenantProvider _tenantProvider;

    public RegulatoryService(
        IRegulatoryRegistry registry,
        ITenantRepository tenantRepo,
        ITenantProvider tenantProvider)
    {
        _registry = registry;
        _tenantRepo = tenantRepo;
        _tenantProvider = tenantProvider;
    }

    public async Task ValidateTransactionAsync(Account account, decimal amount, string operationType)
    {
        var tenant = await _tenantRepo.GetByIdAsync(_tenantProvider.TenantId ?? throw new Exception("Tenant não identificado"));

        if (tenant == null) throw new Exception("Perfil institucional não encontrado.");

        foreach (var jurisdiction in tenant.ActiveJurisdictions)
        {
            var pack = _registry.GetPack(jurisdiction);
            var result = await pack.ValidateTransactionAsync(account, amount, operationType);

            if (!result.Success)
            {
                throw new Exception($"Violação Regulatória ({jurisdiction}): {result.ErrorMessage}");
            }
        }
    }
}
