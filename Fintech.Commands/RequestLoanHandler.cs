using Fintech.Interfaces;
using Fintech.Entities;
using Fintech.Enums;
using Fintech.Core.Interfaces;
using Fintech.ValueObjects;
using Fintech.Application.Commands;

namespace Fintech.Commands;

public class RequestLoanHandler
{
    private readonly ILoanRepository _loanRepo;
    private readonly IFraudDetectionService _fraudService;
    private readonly IAccountRepository _accountRepo;
    private readonly DepositHandler _depositHandler;
    private readonly ITransactionManager _txManager;
    private readonly ITenantProvider _tenantProvider;

    public RequestLoanHandler(
        ILoanRepository loanRepo,
        IFraudDetectionService fraudService,
        IAccountRepository accountRepo,
        DepositHandler depositHandler,
        ITransactionManager txManager,
        ITenantProvider tenantProvider)
    {
        _loanRepo = loanRepo;
        _fraudService = fraudService;
        _accountRepo = accountRepo;
        _depositHandler = depositHandler;
        _txManager = txManager;
        _tenantProvider = tenantProvider;
    }


    public async Task<Guid> Handle(Guid accountId, decimal amount, int installments)
    {
        // Regra simples de elegibilidade
        if (amount > 50000) throw new Exception("Valor solicitado acima do limite automático.");

        // Simulação de Score/Risco
        var isFraudulent = await _fraudService.IsFraudulentAsync(accountId, amount);
        if (isFraudulent) throw new Exception("Solicitação negada por análise de risco.");

        // Criamos o empréstimo
        var tenantId = _tenantProvider.TenantId ?? throw new Exception("TenantId não resolvido.");
        var loan = new Loan(accountId, tenantId, amount, 2.5m, installments);
        loan.Approve(); // Auto-aprovação para Sandbox

        await _loanRepo.AddAsync(loan);

        // Creditamos a conta
        // Como o DepositHandler tem sua própria Transaction/UoW, podemos chamá-lo.
        await _depositHandler.Handle(accountId, amount);

        return loan.Id;
    }
}

