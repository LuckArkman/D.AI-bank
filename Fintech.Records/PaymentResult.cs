namespace Fintech.Records;

public record PaymentResult(
    bool Success,
    string? TransactionId,
    string? ErrorMessage
);