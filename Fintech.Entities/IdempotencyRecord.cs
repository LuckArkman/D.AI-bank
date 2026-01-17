namespace Fintech.Entities;

public class IdempotencyRecord
{
    // O Id será o CommandId ou CorrelationId enviado pelo cliente
    public Guid Id { get; set; } 
    public DateTime ProcessedAt { get; set; }
    public string OperationType { get; set; } // "DEBIT", "TRANSFER"
    public bool Success { get; set; }
    public string ResultJson { get; set; } // Retorno salvo para replay
}