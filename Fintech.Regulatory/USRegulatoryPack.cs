using Fintech.Enums;
using Fintech.Entities;
using Fintech.Core.Entities;
using Fintech.Interfaces;
using Fintech.Regulatory.Rules;

namespace Fintech.Regulatory.Packs;

public class USRegulatoryPack : IRegulatoryPack
{
    private readonly IBusinessRulesEngine _rulesEngine;

    public USRegulatoryPack(IBusinessRulesEngine rulesEngine)
    {
        _rulesEngine = rulesEngine;
    }

    public Jurisdiction Jurisdiction => Jurisdiction.USA;

    public Task<ValidationResult> ValidateTransactionAsync(Account account, decimal amount, string operationType)
    {
        // Example: BSA (Bank Secrecy Act) - Report transactions over 10k USD (simulated validation/warning)
        if (amount > 10000)
        {
            // In a real scenario, this wouldn't block, but trigger a report. 
            // For strict compliance blocking example:
            // return Task.FromResult(new ValidationResult(false, "Transaction requires additional KYC (BSA Rule)"));
        }

        return Task.FromResult(new ValidationResult(true));
    }

    public Task<ValidationResult> ValidateOnboardingAsync(User user)
    {
        // Example: SSN/ITIN required
        if (string.IsNullOrEmpty(user.Email)) // Using Email as proxy for SSN check
        {
            return Task.FromResult(new ValidationResult(false, "SSN/ITIN is required for US residents."));
        }
        return Task.FromResult(new ValidationResult(true));
    }

    public decimal CalculateTax(decimal amount, string operationType)
    {
        // Simulated standard tax if applicable
        return 0;
    }
}
