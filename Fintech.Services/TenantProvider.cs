using Fintech.Interfaces;
using Microsoft.AspNetCore.Http;

namespace Fintech.Services;

public class TenantProvider : ITenantProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public TenantProvider(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid? TenantId
    {
        get
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null) return null;

            // 1. Try to get from User Claims (if authenticated)
            var claim = httpContext.User?.FindFirst("TenantId");
            if (claim != null && Guid.TryParse(claim.Value, out var tenantIdFromClaim))
            {
                return tenantIdFromClaim;
            }

            // 2. Try to get from Header (useful for login/registration/public pages)
            if (httpContext.Request.Headers.TryGetValue("X-Tenant-Id", out var tenantIdHeader))
            {
                if (Guid.TryParse(tenantIdHeader, out var tenantIdFromHeader))
                {
                    return tenantIdFromHeader;
                }
            }

            return null;
        }
    }
}
