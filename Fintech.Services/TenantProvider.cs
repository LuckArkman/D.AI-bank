using Fintech.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

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

            // 1. Prioridade Máxima: Claims do Usuário (Segurança em Produção)
            var claim = httpContext.User?.FindFirst("TenantId");
            if (claim != null && Guid.TryParse(claim.Value, out var tenantIdFromClaim))
            {
                return tenantIdFromClaim;
            }

            // 2. Resolução por Hostname (Cenário Real de Bancos Digitais White-Label)
            // Ex: nubank.dai.com -> Tenant X | inter.dai.com -> Tenant Y
            var host = httpContext.Request.Host.Host.ToLower();
            if (host.Contains("applebank")) return Guid.Parse("d7a5b3c4-e1f2-4a5b-9c8d-7e6f5a4b3c2d");
            if (host.Contains("orangebank")) return Guid.Parse("a1b2c3d4-e5f6-4a5b-9c8d-7e6f5a4b3c2d");

            // 3. Cabeçalho X-Tenant-Id (Integrações e APIs)
            if (httpContext.Request.Headers.TryGetValue("X-Tenant-Id", out var tenantIdHeader))
            {
                if (Guid.TryParse(tenantIdHeader, out var tenantIdFromHeader))
                {
                    return tenantIdFromHeader;
                }
            }

            // 4. Fallback de Desenvolvimento (Apenas para facilitar Swagger local)
            // Em produção, isso seria substituído por um throw new TenantNotFoundException()
            return Guid.Parse("d7a5b3c4-e1f2-4a5b-9c8d-7e6f5a4b3c2d");
        }
    }
}
