using System.ComponentModel.DataAnnotations;

namespace Fintech.Records;

public record InternationalTransferRequest(
    [Required] decimal Amount,
    [Required] string CurrencyCode,
    [Required] string Network, // SWIFT or SEPA
    [Required] string DestinationBank,
    [Required] string DestinationAccount,
    string? Reason = null);
