namespace Fintech.Entities;

public class OutboxMessage
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Topic { get; set; }
    public string PayloadJson { get; set; } // O evento serializado
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool Processed { get; set; } = false;
    public DateTime? ProcessedAt { get; set; }

    public OutboxMessage(string topic, string payloadJson)
    {
        Topic = topic;
        PayloadJson = payloadJson;
        CreatedAt = DateTime.UtcNow;
    }
}