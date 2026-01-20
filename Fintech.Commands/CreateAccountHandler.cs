using Fintech.Core.Interfaces;
using Fintech.Entities;
using Fintech.Interfaces;
using Fintech.Repositories;
using Fintech.Enums;

namespace Fintech.Commands;

public class CreateAccountHandler : ICreateAccountHandler
{
    private readonly IAccountRepository _accountRepo;
    private readonly ITransactionManager _txManager;
    private readonly ILedgerRepository _ledgerRepo;
    private readonly ITenantProvider _tenantProvider;

    public CreateAccountHandler(
        IAccountRepository accountRepo,
        ITransactionManager txManager,
        ILedgerRepository ledgerRepo,
        ITenantProvider tenantProvider)
    {
        _accountRepo = accountRepo;
        _txManager = txManager;
        _ledgerRepo = ledgerRepo;
        _tenantProvider = tenantProvider;
    }

    public async Task<Guid> Handle(decimal initialBalance, AccountProfileType profileType = AccountProfileType.StandardIndividual)
    {
        using var uow = await _txManager.BeginTransactionAsync();
        try
        {
            var tenantId = _tenantProvider.TenantId ?? throw new InvalidOperationException("TenantId não resolvido.");
            var accountId = Guid.NewGuid();
            var account = new Account(accountId, tenantId, profileType);


            if (initialBalance > 0)
            {
                account.Credit(Fintech.ValueObjects.Money.BRL(initialBalance));
            }

            await _accountRepo.AddAsync(account);

            var ledger = new LedgerEvent
            {
                AccountId = accountId,
                TenantId = tenantId,
                Type = "ACCOUNT_CREATED",
                Amount = initialBalance,
                Timestamp = DateTime.UtcNow,
                CorrelationId = Guid.NewGuid()
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
