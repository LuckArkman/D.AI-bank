namespace Fintech.Entities;

public class LedgerEvent
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid AccountId { get; set; }
    public string Type { get; set; } // "DEBIT", "CREDIT"
    public decimal Amount { get; set; }
    public decimal BalanceAfter { get; set; } // Snapshot opcional para auditoria rápida
    public Guid CorrelationId { get; set; } // Rastreabilidade (Saga/Request ID)
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    
    // Metadata para LGPD (Ex: Quem autorizou, IP, Device) - NÃO coloque PII aqui direto
    public Dictionary<string, string> Metadata { get; set; }

    // Construtor vazio para o MongoDB Driver
    public LedgerEvent() { }

    // Construtor utilitário para o código
    public LedgerEvent(Guid accountId, string type, decimal amount, Guid correlationId)
    {
        AccountId = accountId;
        Type = type;
        Amount = amount;
        CorrelationId = correlationId;
    }
}