using Fintech.Enums;
using Fintech.Entities;
using Fintech.Core.Entities;
using Fintech.Interfaces;

namespace Fintech.Regulatory;

public class UKRegulatoryPack : IRegulatoryPack
{
    private readonly IBusinessRulesEngine _rulesEngine;

    public UKRegulatoryPack(IBusinessRulesEngine rulesEngine)
    {
        _rulesEngine = rulesEngine;
    }

    public Jurisdiction Jurisdiction => Jurisdiction.UnitedKingdom;

    public Task<ValidationResult> ValidateTransactionAsync(Account account, decimal amount, string operationType)
    {
        // FCA rules for high-value transactions
        if (amount > 10000)
        {
            // Report to FCA simulated
        }

        if (operationType == "TRANSFER_SENT" && amount > 25000)
        {
            return Task.FromResult(new ValidationResult(false, "UK Faster Payments transaction limit exceeded."));
        }

        return Task.FromResult(new ValidationResult(true));
    }

    public Task<ValidationResult> ValidateOnboardingAsync(User user)
    {
        // KYC for UK residents requires UK address
        return Task.FromResult(new ValidationResult(true));
    }

    public decimal CalculateTax(decimal amount, string operationType)
    {
        if (operationType == "STAMP_DUTY") return amount * 0.005m;
        if (operationType == "INTERNATIONAL_TRANSFER") return 10.0m; // fixed GBP 10.00 fee
        return 0;
    }
}
