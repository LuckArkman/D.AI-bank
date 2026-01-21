using Fintech.Core.Interfaces;

namespace Fintech.Entities;

public class LedgerEvent : IMultiTenant
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public Guid AccountId { get; set; }
    public string Type { get; set; } = string.Empty; // "DEBIT", "CREDIT"
    public decimal Amount { get; set; }
    public string CurrencyCode { get; set; } = "BRL";
    public decimal BalanceAfter { get; set; } // Snapshot opcional para auditoria rápida
    public Guid CorrelationId { get; set; } // Rastreabilidade (Saga/Request ID)
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    // Metadata para LGPD (Ex: Quem autorizou, IP, Device) - NÃO coloque PII aqui direto
    public Dictionary<string, string>? Metadata { get; set; }

    // Construtor vazio para o MongoDB Driver
    public LedgerEvent() { }

    // Construtor utilitário para o código
    public LedgerEvent(Guid accountId, Guid tenantId, string type, decimal amount, string currencyCode, Guid correlationId)
    {
        AccountId = accountId;
        TenantId = tenantId;
        Type = type;
        Amount = amount;
        CurrencyCode = currencyCode;
        CorrelationId = correlationId;
    }
}