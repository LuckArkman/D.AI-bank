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
    private readonly ITenantProvider _tenantProvider;

    public DepositHandler(ITransactionManager txManager, IAccountRepository accountRepo, ILedgerRepository ledgerRepo, ITenantProvider tenantProvider)
    {
        _txManager = txManager;
        _accountRepo = accountRepo;
        _ledgerRepo = ledgerRepo;
        _tenantProvider = tenantProvider;
    }


    public async Task Handle(Guid accountId, decimal amount, string currencyCode = "BRL")
    {
        if (amount <= 0) throw new DomainException("O valor do depósito deve ser positivo.");

        using var uow = await _txManager.BeginTransactionAsync();
        try
        {
            var account = await _accountRepo.GetByIdAsync(accountId);

            // Credita o valor
            var money = Money.Create(amount, currencyCode);
            account.Credit(money);

            await _accountRepo.UpdateAsync(account);

            // Registra no Ledger
            var tenantId = _tenantProvider.TenantId ?? throw new Exception("TenantId não resolvido.");
            await _ledgerRepo.AddAsync(new LedgerEvent(accountId, tenantId, "DEPOSIT_BOLETO", amount, currencyCode, Guid.NewGuid()));

            await uow.CommitAsync();
        }
        catch
        {
            await uow.AbortAsync();
            throw;
        }
    }
}