namespace Fintech.Records;

public record StatementItem(
    DateTime CreatedAt,
    string Type,
    decimal Amount,
    string OperationId
);