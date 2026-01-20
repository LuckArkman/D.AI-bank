namespace Fintech.Records;

// Movimentações
public record P2PTransferRequest(Guid TargetAccountId, decimal Amount);