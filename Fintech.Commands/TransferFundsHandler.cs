using Fintech.Core.Interfaces;
using Fintech.Entities;
using Fintech.Exceptions;
using Fintech.Interfaces;
using Fintech.ValueObjects;
using MongoDB.Driver;
using Fintech.Regulatory;

namespace Fintech.Commands;

public class TransferFundsHandler
{
    private readonly ITransactionManager _txManager;
    private readonly IAccountRepository _accountRepo;
    private readonly ILedgerRepository _ledgerRepo;
    private readonly ITenantProvider _tenantProvider;
    private readonly IRegulatoryService _regulatoryService;

    public TransferFundsHandler(
        ITransactionManager txManager,
        IAccountRepository accountRepo,
        ILedgerRepository ledgerRepo,
        ITenantProvider tenantProvider,
        IRegulatoryService regulatoryService)
    {
        _txManager = txManager;
        _accountRepo = accountRepo;
        _ledgerRepo = ledgerRepo;
        _tenantProvider = tenantProvider;
        _regulatoryService = regulatoryService;
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

            // 2. Regras de Negócio & Compliance Regulatória
            await _regulatoryService.ValidateTransactionAsync(fromAcc, amount, "TRANSFER_SENT");

            fromAcc.Debit(Money.BRL(amount));
            toAcc.Credit(Money.BRL(amount));

            // 3. Persistência
            await _accountRepo.UpdateAsync(fromAcc);
            await _accountRepo.UpdateAsync(toAcc);

            // 4. Ledger (Dupla entrada para rastreabilidade)
            var correlationId = Guid.NewGuid();
            var tenantId = _tenantProvider.TenantId ?? throw new Exception("TenantId não resolvido.");
            await _ledgerRepo.AddAsync(new LedgerEvent(fromAccountId, tenantId, "TRANSFER_SENT", amount, correlationId));
            await _ledgerRepo.AddAsync(new LedgerEvent(toAccountId, tenantId, "TRANSFER_RECEIVED", amount, correlationId));

            await uow.CommitAsync();
        }
        catch
        {
            await uow.AbortAsync();
            throw;
        }
    }
}