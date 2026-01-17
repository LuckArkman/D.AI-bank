namespace Fintech.Consumers;

using Fintech.Interfaces;
using Fintech.Entities;

public class PixProcessConsumer
{
    private readonly ITransactionManager _txManager;
    // Repositórios injetados...

    public async Task Consume(Guid sagaId)
    {
        var saga = await _sagaRepo.GetByIdAsync(sagaId);
        if (saga.Status != PixStatus.BalanceLocked) return;

        try
        {
            // Simulação de chamada externa
            // var result = await _psp.SendPix(...);
            saga.MarkAsCompleted();
        }
        catch
        {
            saga.MarkAsFailed("PSP Error");
            await InitiateRefund(saga);
        }
        await _sagaRepo.UpdateAsync(saga);
    }

    private async Task InitiateRefund(PixSaga saga)
    {
        using var uow = await _txManager.BeginTransactionAsync();
        var account = await _accountRepo.GetByIdAsync(saga.AccountId);
        
        // Devolve o dinheiro (Compensação)
        // account.Credit(saga.Amount);
        
        await _accountRepo.UpdateAsync(account);
        saga.MarkAsRefunded();
        await _sagaRepo.UpdateAsync(saga);
        await uow.CommitAsync();
    }
}