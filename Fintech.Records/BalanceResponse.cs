namespace Fintech.Records;

public record BalanceResponse(Guid AccountId, decimal AvailableBalance);