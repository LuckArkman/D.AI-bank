using Fintech.Core.Interfaces;

namespace Fintech.Entities;

public class PixKey : IMultiTenant
{
    public string Key { get; private set; } // PK
    public string Type { get; private set; } // CPF, EMAIL, RANDOM
    public Guid TenantId { get; private set; }
    public Guid AccountId { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public PixKey(string key, string type, Guid tenantId, Guid accountId)
    {
        Key = key;
        Type = type;
        TenantId = tenantId;
        AccountId = accountId;
        CreatedAt = DateTime.UtcNow;
    }
}