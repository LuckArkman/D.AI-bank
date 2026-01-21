namespace Fintech.Interfaces;

public interface IFraudDetectionService
{
    Task<bool> IsTransactionFraudulentAsync(Guid accountId, decimal amount, string? metadata);
    Task<bool> IsFraudulentAsync(Guid accountId, decimal amount);
}

