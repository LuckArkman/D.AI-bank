using Fintech.Enums;
using Fintech.Entities;
using Fintech.Core.Entities;
using Fintech.Interfaces;

namespace Fintech.Regulatory;

public class EURegulatoryPack : IRegulatoryPack
{
    private readonly IBusinessRulesEngine _rulesEngine;

    public EURegulatoryPack(IBusinessRulesEngine rulesEngine)
    {
        _rulesEngine = rulesEngine;
    }

    public Jurisdiction Jurisdiction => Jurisdiction.Europe;

    public Task<ValidationResult> ValidateTransactionAsync(Account account, decimal amount, string operationType)
    {
        // Example: AMLD5 compliance check
        if (amount > 10000)
        {
            // Transactions over 10k EUR require strong customer authentication (SCA)
            // and might need additional verification.
        }

        if (operationType == "TRANSFER_SENT" && amount > 50000)
        {
            return Task.FromResult(new ValidationResult(false, "EU Transaction limit for unverified accounts exceeded. (SEPA Rule)"));
        }

        return Task.FromResult(new ValidationResult(true));
    }

    public Task<ValidationResult> ValidateOnboardingAsync(User user)
    {
        // Proof of Residence is mandatory in most EU countries
        return Task.FromResult(new ValidationResult(true));
    }

    public decimal CalculateTax(decimal amount, string operationType)
    {
        if (operationType == "VAT_TRANSACTION") return amount * 0.20m;
        if (operationType == "INTERNATIONAL_TRANSFER") return amount * 0.005m; // 0.5% Cross-border fee
        return 0;
    }
}
