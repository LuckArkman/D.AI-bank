using Fintech.Application.Commands;
using Fintech.Core.Interfaces;
using Fintech.Entities;
using Fintech.Enums;
using Fintech.Interfaces;

namespace Fintech.Commands;

public class InvestmentHandler
{
    private readonly IInvestmentRepository _investmentRepo;
    private readonly IAccountRepository _accountRepo;
    private readonly ITransactionManager _txManager;
    private readonly ILedgerRepository _ledgerRepo;

    public InvestmentHandler(
        IInvestmentRepository investmentRepo,
        IAccountRepository accountRepo,
        ITransactionManager txManager,
        ILedgerRepository ledgerRepo)
    {
        _investmentRepo = investmentRepo;
        _accountRepo = accountRepo;
        _txManager = txManager;
        _ledgerRepo = ledgerRepo;
    }

    public async Task InvestAsync(Guid accountId, string name, InvestmentType type, decimal amount)
    {
        using var uow = await _txManager.BeginTransactionAsync();
        try
        {
            var account = await _accountRepo.GetByIdAsync(accountId);
            if (account == null) throw new Exception("Conta não encontrada.");

            // Debita da conta
            account.Debit(Fintech.ValueObjects.Money.BRL(amount));
            await _accountRepo.UpdateAsync(account);

            // Cria investimento
            var investment = new Investment(accountId, name, type, amount, 10.5m); // Taxa fixa simulada
            await _investmentRepo.AddInvestmentAsync(investment);

            // Ledger
            await _ledgerRepo.AddAsync(new LedgerEvent(accountId, "INVESTMENT_DEBIT", amount, Guid.NewGuid()));

            await uow.CommitAsync();
        }
        catch
        {
            await uow.AbortAsync();
            throw;
        }
    }

    public async Task CreateGoalAsync(Guid accountId, string name, decimal targetAmount, string color)
    {
        var goal = new SavingsGoal(accountId, name, targetAmount, color);
        await _investmentRepo.AddGoalAsync(goal);
    }

    public async Task AddToGoalAsync(Guid accountId, Guid goalId, decimal amount)
    {
        using var uow = await _txManager.BeginTransactionAsync();
        try
        {
            var account = await _accountRepo.GetByIdAsync(accountId);
            var goal = await _investmentRepo.GetGoalByIdAsync(goalId);

            if (account == null || goal == null || goal.AccountId != accountId)
                throw new Exception("Dados inválidos.");

            account.Debit(Fintech.ValueObjects.Money.BRL(amount));
            await _accountRepo.UpdateAsync(account);

            goal.AddFunds(amount);
            await _investmentRepo.UpdateGoalAsync(goal);

            await _ledgerRepo.AddAsync(new LedgerEvent(accountId, "GOAL_DEPOSIT", amount, Guid.NewGuid()));

            await uow.CommitAsync();
        }
        catch
        {
            await uow.AbortAsync();
            throw;
        }
    }
}
