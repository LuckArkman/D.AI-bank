using Fintech.ValueObjects;

namespace Fintech.Interfaces;

public record SettlementResponse(bool Success, string TransactionReference, string? ErrorMessage = null);

public interface ISettlementGateway
{
    string Network { get; }
    Task<SettlementResponse> ProcessSettlementAsync(Guid transactionId, Money amount, string destinationBank, string destinationAccount);
}
