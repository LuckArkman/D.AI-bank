using Fintech.Interfaces;
using Microsoft.Extensions.Logging;

namespace Fintech.Services.ProductModules;

public class CardsModule : IProductModule
{
    private readonly ILogger<CardsModule> _logger;

    public CardsModule(ILogger<CardsModule> logger)
    {
        _logger = logger;
    }

    public string ModuleId => "cards";
    public string Name => "Credit & Debit Cards";
    public bool IsActive => true;

    public Task InitializeAsync(Guid tenantId)
    {
        _logger.LogInformation("Initializing Cards Module for Tenant {TenantId}", tenantId);
        // Setup card issuer integration configs
        return Task.CompletedTask;
    }

    public Task DisableAsync(Guid tenantId)
    {
        _logger.LogInformation("Disabling Cards Module for Tenant {TenantId}", tenantId);
        return Task.CompletedTask;
    }
}
