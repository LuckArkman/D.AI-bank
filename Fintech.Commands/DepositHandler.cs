using Fintech.Core.Interfaces;
using Fintech.Entities;
using Fintech.Interfaces;
using Fintech.ValueObjects;
using Fintech.Exceptions;

namespace Fintech.Application.Commands;

public class DepositHandler
{
    private readonly ITransactionManager _txManager;
    private readonly IAccountRepository _accountRepo;
    private readonly ILedgerRepository _ledgerRepo;

    public DepositHandler(ITransactionManager txManager, IAccountRepository accountRepo, ILedgerRepository ledgerRepo)
    {
        _txManager = txManager;
        _accountRepo = accountRepo;
        _ledgerRepo = ledgerRepo;
    }


    public async Task Handle(Guid accountId, decimal amount)
    {
        if (amount <= 0) throw new DomainException("O valor do depósito deve ser positivo.");

        using var uow = await _txManager.BeginTransactionAsync();
        try
        {
            var account = await _accountRepo.GetByIdAsync(accountId);

            // Credita o valor
            account.Credit(Money.BRL(amount));

            await _accountRepo.UpdateAsync(account);

            // Registra no Ledger
            await _ledgerRepo.AddAsync(new LedgerEvent(accountId, "DEPOSIT_BOLETO", amount, Guid.NewGuid()));

            await uow.CommitAsync();
        }
        catch
        {
            await uow.AbortAsync();
            throw;
        }
    }
}