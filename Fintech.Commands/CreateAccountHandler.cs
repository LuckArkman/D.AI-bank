using Fintech.Entities;
using Fintech.Interfaces;
using Fintech.Repositories;

namespace Fintech.Commands;

public class CreateAccountHandler : ICreateAccountHandler
{
    private readonly AccountRepository _accountRepo;
    private readonly ITransactionManager _txManager;
    private readonly LedgerRepository _ledgerRepo;

    public CreateAccountHandler(
        AccountRepository accountRepo,
        ITransactionManager txManager,
        LedgerRepository ledgerRepo)
    {
        _accountRepo = accountRepo;
        _txManager = txManager;
        _ledgerRepo = ledgerRepo;
    }

    public async Task<Guid> Handle(decimal initialBalance)
    {
        using var uow = await _txManager.BeginTransactionAsync();
        try
        {
            var accountId = Guid.NewGuid();
            var account = new Account(accountId);

            await _accountRepo.AddAsync(account);

            var ledger = new LedgerEvent
            {
                AccountId = accountId,
                Type = "ACCOUNT_CREATED",
                Amount = initialBalance,
                Timestamp = DateTime.UtcNow
            };
            await _ledgerRepo.AddAsync(ledger);

            await uow.CommitAsync();
            return accountId;
        }
        catch
        {
            await uow.AbortAsync();
            throw;
        }
    }
}