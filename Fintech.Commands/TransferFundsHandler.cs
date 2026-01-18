using Fintech.Core.Interfaces;
using Fintech.Entities;
using Fintech.Exceptions;
using Fintech.Interfaces;
using Fintech.ValueObjects;
using MongoDB.Driver;

namespace Fintech.Commands;

public class TransferFundsHandler
{
    private readonly ITransactionManager _txManager;
    private readonly IAccountRepository _accountRepo;
    private readonly ILedgerRepository _ledgerRepo;

    public TransferFundsHandler(
        ITransactionManager txManager,
        IAccountRepository accountRepo,
        ILedgerRepository ledgerRepo)
    {
        _txManager = txManager;
        _accountRepo = accountRepo;
        _ledgerRepo = ledgerRepo;
    }

    public async Task Handle(Guid fromAccountId, Guid toAccountId, decimal amount)
    {
        if (fromAccountId == toAccountId) throw new DomainException("Origem e destino iguais.");

        using var uow = await _txManager.BeginTransactionAsync();
        try
        {
            // 1. Carrega as duas contas (Lock otimista será aplicado no Update)
            var fromAcc = await _accountRepo.GetByIdAsync(fromAccountId);
            var toAcc = await _accountRepo.GetByIdAsync(toAccountId);

            // 2. Regras de Negócio
            fromAcc.Debit(Money.BRL(amount));
            toAcc.Credit(Money.BRL(amount));

            // 3. Persistência
            await _accountRepo.UpdateAsync(fromAcc);
            await _accountRepo.UpdateAsync(toAcc);

            // 4. Ledger (Dupla entrada para rastreabilidade)
            var correlationId = Guid.NewGuid();
            await _ledgerRepo.AddAsync(new LedgerEvent(fromAccountId, "TRANSFER_SENT", amount, correlationId));
            await _ledgerRepo.AddAsync(new LedgerEvent(toAccountId, "TRANSFER_RECEIVED", amount, correlationId));

            await uow.CommitAsync();
        }
        catch
        {
            await uow.AbortAsync();
            throw;
        }
    }
}