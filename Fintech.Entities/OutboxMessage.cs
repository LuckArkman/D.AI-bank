using Fintech.Core.Interfaces;

namespace Fintech.Entities;

public class OutboxMessage : IMultiTenant
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public string Topic { get; set; }
    public string PayloadJson { get; set; } // O evento serializado
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool Processed { get; set; } = false;
    public DateTime? ProcessedAt { get; set; }
    public string? LockedBy { get; set; }
    public DateTime? LockedAt { get; set; }


    public OutboxMessage(string topic, string payloadJson, Guid tenantId)
    {
        Topic = topic;
        PayloadJson = payloadJson;
        TenantId = tenantId;
        CreatedAt = DateTime.UtcNow;
    }
}