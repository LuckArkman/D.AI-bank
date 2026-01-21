using Fintech.Enums;
using Fintech.Entities;
using Fintech.Core.Entities;

namespace Fintech.Regulatory;

public interface IRegulatoryPack
{
    Jurisdiction Jurisdiction { get; }
    Task<ValidationResult> ValidateTransactionAsync(Account account, decimal amount, string operationType);
    Task<ValidationResult> ValidateOnboardingAsync(User user);
    decimal CalculateTax(decimal amount, string operationType);
}

public record ValidationResult(bool Success, string? ErrorMessage = null);
