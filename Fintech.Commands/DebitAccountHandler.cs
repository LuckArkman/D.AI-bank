using Fintech.Core.Interfaces;
using Fintech.Entities;
using Fintech.Interfaces;
using Fintech.Telemetry;
using Fintech.ValueObjects;
using System.Text.Json;

namespace Fintech.Commands;

public class DebitAccountHandler
{
    private readonly ITransactionManager _txManager;
    private readonly IAccountRepository _accountRepo;
    private readonly IOutboxRepository _outboxRepo;
    private readonly ITenantProvider _tenantProvider;

    public DebitAccountHandler(
        ITransactionManager txManager,
        IAccountRepository accountRepo,
        IOutboxRepository outboxRepo,
        ITenantProvider tenantProvider)
    {
        _txManager = txManager;
        _accountRepo = accountRepo;
        _outboxRepo = outboxRepo;
        _tenantProvider = tenantProvider;
    }


    public async Task Handle(Guid accountId, decimal amount, Guid correlationId)
    {
        using var uow = await _txManager.BeginTransactionAsync();
        try
        {
            var account = await _accountRepo.GetByIdAsync(accountId);

            // Correção: Converter decimal para Money
            account.Debit(Money.BRL(amount));

            await _accountRepo.UpdateAsync(account);

            var payload = JsonSerializer.Serialize(new { AccountId = accountId, Amount = amount, CorrelationId = correlationId });
            var tenantId = _tenantProvider.TenantId ?? throw new Exception("TenantId não resolvido.");
            var msg = new OutboxMessage("AccountDebited", payload, tenantId);
            await _outboxRepo.AddAsync(msg);

            await uow.CommitAsync();
            FintechMetrics.RecordDebit(amount);
            FintechMetrics.RecordSuccess();
        }
        catch
        {
            Fintech.Telemetry.FintechMetrics.RecordFailure();
            await uow.AbortAsync();
            throw;
        }
    }
}