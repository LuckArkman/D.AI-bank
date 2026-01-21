using Fintech.Enums;
using Fintech.Entities;
using Fintech.Core.Entities;
using Fintech.Interfaces;

namespace Fintech.Regulatory;

public class BrazilRegulatoryPack : IRegulatoryPack
{
    private readonly IBusinessRulesEngine _rulesEngine;

    public BrazilRegulatoryPack(IBusinessRulesEngine rulesEngine)
    {
        _rulesEngine = rulesEngine;
    }

    public Jurisdiction Jurisdiction => Jurisdiction.Brazil;

    public Task<ValidationResult> ValidateTransactionAsync(Account account, decimal amount, string operationType)
    {
        // Exemplo de regra do BACEN: limite de transferência noturna
        var hour = DateTime.Now.Hour;
        if ((hour >= 20 || hour < 6) && amount > 1000)
        {
            return Task.FromResult(new ValidationResult(false, "Limite noturno excedido (Regra BACEN)."));
        }

        return Task.FromResult(new ValidationResult(true));
    }

    public Task<ValidationResult> ValidateOnboardingAsync(User user)
    {
        // Exemplo: CPF obrigatório no Brasil
        if (string.IsNullOrEmpty(user.Email)) // Simulação, deveria ser CPF
        {
            return Task.FromResult(new ValidationResult(false, "Documento obrigatório para residentes no Brasil."));
        }

        return Task.FromResult(new ValidationResult(true));
    }

    public decimal CalculateTax(decimal amount, string operationType)
    {
        if (operationType == "INVESTMENT_GAIN") return amount * 0.15m;
        if (operationType == "INTERNATIONAL_TRANSFER") return amount * 0.011m; // 1.1% IOF
        return 0;
    }
}
