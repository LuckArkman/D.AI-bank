using Fintech.Interfaces;
using Microsoft.Extensions.Logging;

namespace Fintech.Services.ProductModules;

public class LoansModule : IProductModule
{
    private readonly ILogger<LoansModule> _logger;

    public LoansModule(ILogger<LoansModule> logger)
    {
        _logger = logger;
    }

    public string ModuleId => "loans";
    public string Name => "Consumer Loans";
    public bool IsActive => true;

    public Task InitializeAsync(Guid tenantId)
    {
        _logger.LogInformation("Initializing Loans Module for Tenant {TenantId}", tenantId);
        // Setup loan risk engine parameters
        return Task.CompletedTask;
    }

    public Task DisableAsync(Guid tenantId)
    {
        _logger.LogInformation("Disabling Loans Module for Tenant {TenantId}", tenantId);
        return Task.CompletedTask;
    }
}
