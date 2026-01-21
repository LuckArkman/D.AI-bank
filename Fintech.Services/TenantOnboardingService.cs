using Fintech.DTOs;
using Fintech.Interfaces;
using Fintech.Entities;
using Fintech.Core.Interfaces;
using Fintech.Core.Entities;
using Fintech.Enums;

namespace Fintech.Services;

public class TenantOnboardingService : ITenantOnboardingService
{
    private readonly ITenantRepository _tenantRepo;
    private readonly ICreateAccountHandler _createAccountHandler;
    private readonly IUserRepository _userRepo;
    private readonly ITransactionManager _txManager;
    private readonly IEnumerable<IProductModule> _productModules;

    public TenantOnboardingService(
        ITenantRepository tenantRepo,
        ICreateAccountHandler createAccountHandler,
        IUserRepository userRepo,
        ITransactionManager txManager,
        IEnumerable<IProductModule> productModules)
    {
        _tenantRepo = tenantRepo;
        _createAccountHandler = createAccountHandler;
        _userRepo = userRepo;
        _txManager = txManager;
        _productModules = productModules;
    }

    public async Task<TenantOnboardingResponse> OnboardAsync(TenantOnboardingRequest request)
    {
        // 1. Create Tenant
        var tenant = new Tenant(
            request.Name,
            request.Identifier,
            request.Branding,
            request.DefaultCurrency ?? "BRL",
            request.TimeZoneId ?? "E. South America Standard Time");

        foreach (var jur in request.Jurisdictions) tenant.AddJurisdiction(jur);
        foreach (var mode in request.BusinessModes) tenant.AddBusinessMode(mode);

        // Activate Products
        if (request.ActiveProducts != null)
        {
            foreach (var moduleId in request.ActiveProducts)
            {
                var module = _productModules.FirstOrDefault(m => m.ModuleId == moduleId);
                if (module != null)
                {
                    await module.InitializeAsync(tenant.Id);
                    tenant.ActivateProduct(moduleId);
                }
            }
        }

        await _tenantRepo.AddAsync(tenant);

        // 2. Create Admin User via Transaction
        using var uow = await _txManager.BeginTransactionAsync();
        try
        {
            // Create Bank Account for Admin using the specific TenantId
            var accountId = await _createAccountHandler.Handle(0, tenant.Id, AccountProfileType.Business);

            // Create Admin User
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.AdminPassword);
            var adminUser = new User("Admin", request.AdminEmail, passwordHash, accountId, tenant.Id);

            await _userRepo.AddAsync(adminUser);

            await uow.CommitAsync();

            // Generate API Key (placeholder logic)
            var apiKey = Convert.ToBase64String(Guid.NewGuid().ToByteArray());

            return new TenantOnboardingResponse(tenant.Id, adminUser.Id, apiKey);
        }
        catch
        {
            await uow.AbortAsync();
            throw;
        }
    }
}
