using System.Net.Http.Json;
using Fintech.Enums;
using Fintech.Repositories;
using Fintech.ValueObjects;
using Fintech.Entities;
using Fintech.Interfaces;
using Fintech.Core.Interfaces;
using Fintech.Records; // Adicionado para ITransactionManager

namespace Fintech.Consumers;

public class PixProcessConsumer
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly LedgerRepository _ledgerRepo;
    private readonly SagaRepository _sagaRepo;
    private readonly AccountRepository _accountRepo;
    private readonly ITransactionManager _txManager; // Campo adicionado
    private readonly ITenantProvider _tenantProvider;

    // Construtor adicionado para Injeção de Dependência
    public PixProcessConsumer(
        IHttpClientFactory httpClientFactory,
        LedgerRepository ledgerRepo,
        SagaRepository sagaRepo,
        AccountRepository accountRepo,
        ITransactionManager txManager,
        ITenantProvider tenantProvider)
    {
        _httpClientFactory = httpClientFactory;
        _ledgerRepo = ledgerRepo;
        _sagaRepo = sagaRepo;
        _accountRepo = accountRepo;
        _txManager = txManager;
        _tenantProvider = tenantProvider;
    }

    public async Task Consume(PixExternalMessage msg)
    {
        var saga = await _sagaRepo.GetByIdAsync(msg.SagaId);
        if (saga == null || saga.Status != PixStatus.BalanceLocked) return; // Idempotência e Null Check

        var client = _httpClientFactory.CreateClient("CentralBank");

        try
        {
            var response = await client.PostAsJsonAsync("api/spi/payment",
                new { Key = saga.PixKey, Amount = saga.Amount });

            if (response.IsSuccessStatusCode)
            {
                saga.MarkAsCompleted();
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                await InitiateRefund(saga, "Chave Pix Inválida");
            }
            else
            {
                await InitiateRefund(saga, "Banco Central Indisponível");
            }
        }
        catch (Exception ex)
        {
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
        using var uow = await _txManager.BeginTransactionAsync();

        var account = await _accountRepo.GetByIdAsync(saga.AccountId);

        // Usa Money Value Object
        account.Credit(Money.Create(saga.Amount, saga.CurrencyCode));

        var tenantId = _tenantProvider.TenantId ?? Guid.Empty;
        await _accountRepo.UpdateAsync(account);
        await _ledgerRepo.AddAsync(new LedgerEvent(saga.AccountId, tenantId, "PIX_REFUND", saga.Amount, saga.CurrencyCode, saga.Id));

        await uow.CommitAsync();

        Console.WriteLine($"[SAGA] Estorno realizado para Saga {saga.Id}. Motivo: {reason}");
    }
}
