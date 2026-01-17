using System.Net.Http.Json;
using Fintech.Enums;
using Fintech.Repositories;
using Fintech.ValueObjects;

namespace Fintech.Consumers;

using Fintech.Interfaces;
using Fintech.Entities;

public class PixProcessConsumer
{
    private readonly IHttpClientFactory _httpClientFactory;
    readonly LedgerRepository _ledgerRepo;
    private readonly SagaRepository _sagaRepo;
    private readonly AccountRepository _accountRepo; // Para estorno
    // ... construtor

    public async Task Consume(PixExternalMessage msg)
    {
        var saga = await _sagaRepo.GetByIdAsync(msg.SagaId);
        if (saga.Status != PixStatus.BalanceLocked) return; // Idempotência

        var client = _httpClientFactory.CreateClient("CentralBank");
        
        try
        {
            // O Polly está "envolvendo" esta chamada. 
            // Se lançar exception aqui, é porque as 3 tentativas falharam.
            var response = await client.PostAsJsonAsync("api/spi/payment", 
                new { Key = saga.PixKey, Amount = saga.Amount });

            if (response.IsSuccessStatusCode)
            {
                saga.MarkAsCompleted();
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.BadRequest) // Erro 400
            {
                // Erro definitivo (Chave inválida). Não adianta tentar de novo.
                // AÇÃO: ESTORNO (Refund)
                await InitiateRefund(saga, "Chave Pix Inválida");
            }
            else
            {
                // Erro 500 persistente (após retries)
                // AÇÃO: ESTORNO (Refund) ou jogar para DLQ para análise manual
                // Aqui vamos estornar para liberar o saldo do cliente.
                await InitiateRefund(saga, "Banco Central Indisponível");
            }
        }
        catch (Exception ex)
        {
            // Timeout ou Circuit Breaker aberto
            await InitiateRefund(saga, $"Falha de Comunicação: {ex.Message}");
        }

        await _sagaRepo.UpdateAsync(saga);
    }

    private async Task InitiateRefund(PixSaga saga, string reason)
    {
        // 1. Atualiza Saga
        saga.MarkAsFailed(reason);
        saga.MarkAsRefunded();

        // 2. Estorno no Ledger e Saldo (Transação ACID)
        // Nota: Em código real, use ITransactionManager aqui igual ao Handler
        using var uow = await _txManager.BeginTransactionAsync();
        
        var account = await _accountRepo.GetByIdAsync(saga.AccountId);
        account.Credit(Money.BRL(saga.Amount)); // Devolve o dinheiro
        
        await _accountRepo.UpdateAsync(account);
        await _ledgerRepo.AddAsync(new LedgerEvent(saga.AccountId, "PIX_REFUND", saga.Amount, saga.Id));
        
        await uow.CommitAsync();
        
        Console.WriteLine($"[SAGA] Estorno realizado para Saga {saga.Id}. Motivo: {reason}");
    }
}