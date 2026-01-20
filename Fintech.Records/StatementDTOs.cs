namespace Fintech.Records;

public record StatementResponse(
    decimal CurrentBalance,
    List<StatementItem> Transactions
);