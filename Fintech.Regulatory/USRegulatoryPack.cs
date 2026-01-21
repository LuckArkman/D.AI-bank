using Fintech.Enums;
using Fintech.Entities;
using Fintech.Core.Entities;
using Fintech.Interfaces;

namespace Fintech.Regulatory;

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
        // Example: BSA (Bank Secrecy Act) - Report transactions over 10k USD
        if (amount > 10000)
        {
            // Report to FinCEN simulated
        }

        if (operationType == "TRANSFER_SENT" && amount > 50000)
        {
            return Task.FromResult(new ValidationResult(false, "ACH high-value transaction requires additional review. (Regulation E)"));
        }

        return Task.FromResult(new ValidationResult(true));
    }

    public Task<ValidationResult> ValidateOnboardingAsync(User user)
    {
        // Example: SSN/ITIN required for US residents
        return Task.FromResult(new ValidationResult(true));
    }

    public decimal CalculateTax(decimal amount, string operationType)
    {
        // Note: US tax is complex, this is just a placeholder for a specific state tax simulation
        if (operationType == "STATE_TAX_NY") return amount * 0.08875m;
        return 0;
    }
}
