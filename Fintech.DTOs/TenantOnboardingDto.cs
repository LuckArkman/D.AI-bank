using Fintech.Enums;
using Fintech.Entities;

namespace Fintech.DTOs;

public record TenantOnboardingRequest(
    string Name,
    string Identifier,
    string AdminEmail,
    string AdminPassword,
    string DefaultCurrency,
    string TimeZoneId,
    List<Jurisdiction> Jurisdictions,
    List<BusinessMode> BusinessModes,
    List<string> ActiveProducts,
    TenantBranding Branding
);

public record TenantOnboardingResponse(
    Guid TenantId,
    Guid AdminUserId,
    string ApiKey
);
