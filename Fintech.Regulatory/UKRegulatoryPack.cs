using Fintech.Enums;
using Fintech.Entities;
using Fintech.Core.Entities;
using Fintech.Interfaces;
using Fintech.Regulatory.Rules;
using Fintech.Interfaces;

namespace Fintech.Regulatory.Packs;

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
        // Example: Faster Payments internal limit check
        if (amount > 1000000) // 1 Million GBP
        {
            return Task.FromResult(new ValidationResult(false, "Transaction exceeds Faster Payments limit."));
        }
        return Task.FromResult(new ValidationResult(true));
    }

    public Task<ValidationResult> ValidateOnboardingAsync(User user)
    {
        return Task.FromResult(new ValidationResult(true));
    }

    public decimal CalculateTax(decimal amount, string operationType)
    {
        return 0;
    }
}
