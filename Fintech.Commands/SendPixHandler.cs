using Fintech.Entities;
using Fintech.Interfaces;
using Fintech.Core.Interfaces;

namespace Fintech.Commands;

public class SendPixHandler
{
    private readonly ISagaRepository _sagaRepo;
    private readonly ITransactionManager _txManager;
    private readonly IPixOrchestrator _orchestrator;
    private readonly ITenantProvider _tenantProvider;

    public SendPixHandler(ISagaRepository sagaRepo, ITransactionManager txManager, IPixOrchestrator orchestrator, ITenantProvider tenantProvider)
    {
        _sagaRepo = sagaRepo;
        _txManager = txManager;
        _orchestrator = orchestrator;
        _tenantProvider = tenantProvider;
    }


    public async Task<Guid> Handle(Guid accountId, string pixKey, decimal amount, string currencyCode = "BRL")
    {
        var tenantId = _tenantProvider.TenantId ?? throw new Exception("TenantId não resolvido.");
        var saga = new PixSaga(accountId, tenantId, amount, currencyCode)
        {
            PixKey = pixKey
        };

        using var uow = await _txManager.BeginTransactionAsync();


        await _sagaRepo.AddAsync(saga);

        // Dispara o primeiro passo da Saga
        await _orchestrator.ProcessPixSaga(saga.Id);

        await uow.CommitAsync();

        return saga.Id;
    }
}