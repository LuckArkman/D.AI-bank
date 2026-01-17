namespace Fintech.Services;

public class PixOrchestrator
{
    public async Task ProcessPixSaga(Guid sagaId)
    {
        var saga = await _sagas.Find(x => x.Id == sagaId).FirstOrDefaultAsync();

        switch (saga.CurrentState)
        {
            case "Created":
                // Passo 1: Bloquear Saldo (Reserva)
                // Chama DebitAccountHandler mas com Type="LOCK"
                try {
                    await _accountService.LockFunds(saga.AccountId, saga.Amount);
                    saga.CurrentState = "BalanceLocked";
                } catch {
                    saga.CurrentState = "Failed"; // Saldo insuficiente
                }
                break;

            case "BalanceLocked":
                // Passo 2: Chamar API do Banco Central (Idempotente)
                try {
                    var response = await _pixGateway.SendPixAsync(saga.PixKey, saga.Amount);
                    if (response.Success) saga.CurrentState = "PixSent";
                    else {
                        saga.CurrentState = "PixFailed";
                        // Dispara evento de compensação (estorno)
                    }
                } catch (TimeoutException) {
                    // Não muda estado, Worker vai tentar de novo (Retry)
                    return; 
                }
                break;

            case "PixSent":
                // Passo 3: Efetivar Ledger (Transformar Lock em Débito real)
                await _ledgerService.ConfirmDebit(saga.AccountId, saga.Amount);
                saga.CurrentState = "Completed";
                break;
            
            case "PixFailed":
                // Passo 3 (Alternativo): Estornar o Lock
                await _accountService.UnlockFunds(saga.AccountId, saga.Amount);
                saga.CurrentState = "Failed";
                break;
        }

        await _sagas.ReplaceOneAsync(x => x.Id == saga.Id, saga);
    
        // Se não terminou, joga no Outbox para o Worker pegar e processar o próximo passo
        if (saga.CurrentState != "Completed" && saga.CurrentState != "Failed")
        {
            _outbox.Enqueue($"continue-saga-{saga.Id}");
        }
    }
}