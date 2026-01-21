using Fintech.DTOs;

namespace Fintech.Interfaces;

public interface ITenantOnboardingService
{
    Task<TenantOnboardingResponse> OnboardAsync(TenantOnboardingRequest request);
}
