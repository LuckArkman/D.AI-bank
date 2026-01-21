using Fintech.Entities;
using Fintech.Core.Interfaces;
using Fintech.Enums;
using Fintech.Interfaces;
using Fintech.ValueObjects;

namespace Fintech.Services;

public class PixOrchestrator : IPixOrchestrator
{
    private readonly ISagaRepository _sagaRepo;
    private readonly IAccountRepository _accountRepo;
    private readonly IPixGateway _pixGateway;
    private readonly IOutboxRepository _outboxRepo;
    private readonly ITenantProvider _tenantProvider;

    public PixOrchestrator(
        ISagaRepository sagaRepo,
        IAccountRepository accountRepo,
        IPixGateway pixGateway,
        IOutboxRepository outboxRepo,
        ITenantProvider tenantProvider)
    {
        _sagaRepo = sagaRepo;
        _accountRepo = accountRepo;
        _pixGateway = pixGateway;
        _outboxRepo = outboxRepo;
        _tenantProvider = tenantProvider;
    }


    public async Task ProcessPixSaga(Guid sagaId)
    {
        var saga = await _sagaRepo.GetByIdAsync(sagaId);
        if (saga == null) return;

        switch (saga.Status)
        {
            case PixStatus.Created:
                await HandleCreated(saga);
                break;

            case PixStatus.BalanceLocked:
                await HandleBalanceLocked(saga);
                break;
        }

        await _sagaRepo.UpdateAsync(saga);
    }

    private async Task HandleCreated(PixSaga saga)
    {
        try
        {
            var account = await _accountRepo.GetByIdAsync(saga.AccountId);
            account.Debit(Money.Create(saga.Amount, saga.CurrencyCode));
            await _accountRepo.UpdateAsync(account);

            saga.MarkAsLocked();

            // Agenda o próximo passo via Outbox para processamento assíncrono
            var tenantId = _tenantProvider.TenantId ?? throw new Exception("TenantId não resolvido.");
            await _outboxRepo.AddAsync(new OutboxMessage("pix-saga-locked", saga.Id.ToString(), tenantId));
        }
        catch (Exception ex)
        {
            saga.MarkAsFailed(ex.Message);
        }
    }

    private async Task HandleBalanceLocked(PixSaga saga)
    {
        try
        {
            var response = await _pixGateway.SendPixAsync(saga.PixKey, saga.Amount);


            if (response.Success)
            {
                saga.MarkAsCompleted();
            }
            else
            {
                saga.MarkAsFailed(response.ErrorCode ?? "Gateway Error");
                // Disparar compensação (refund) seria o próximo passo ou feito aqui
                await Compensate(saga);
            }
        }
        catch (Exception)
        {
            // Em caso de erro técnico amigável ou timeout, o worker de retentativa pegará o lock.
        }
    }

    private async Task Compensate(PixSaga saga)
    {
        var account = await _accountRepo.GetByIdAsync(saga.AccountId);
        account.Credit(Money.Create(saga.Amount, saga.CurrencyCode));
        await _accountRepo.UpdateAsync(account);
        saga.MarkAsRefunded();
    }
}
