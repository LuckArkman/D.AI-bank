using Fintech.Entities;
using Fintech.Enums;
using Fintech.Interfaces;

namespace Fintech.Commands;

public class RequestLoanHandler
{
    private readonly ILoanRepository _loanRepo;
    private readonly IFraudDetectionService _fraudService;

    public RequestLoanHandler(ILoanRepository loanRepo, IFraudDetectionService fraudService)
    {
        _loanRepo = loanRepo;
        _fraudService = fraudService;
    }

    public async Task<Guid> Handle(Guid accountId, decimal amount, int installments)
    {
        // Regra simples de elegibilidade
        if (amount > 50000) throw new Exception("Valor solicitado acima do limite automático.");

        // Simulação de Score/Risco
        var isFraudulent = await _fraudService.IsFraudulentAsync(accountId, amount);
        if (isFraudulent) throw new Exception("Solicitação negada por análise de risco.");

        var loan = new Loan(accountId, amount, 2.5m, installments); // 2.5% de juros

        await _loanRepo.AddAsync(loan);
        return loan.Id;
    }
}
