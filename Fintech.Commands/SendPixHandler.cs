using Fintech.Entities;
using Fintech.Interfaces;
using Fintech.Repositories;
using Fintech.Services;

namespace Fintech.Commands;

public class SendPixHandler
{
    private readonly SagaRepository _sagaRepo;
    private readonly ITransactionManager _txManager;
    private readonly PixOrchestrator _orchestrator;

    public SendPixHandler(SagaRepository sagaRepo, ITransactionManager txManager, PixOrchestrator orchestrator)
    {
        _sagaRepo = sagaRepo;
        _txManager = txManager;
        _orchestrator = orchestrator;
    }

    public async Task<Guid> Handle(Guid accountId, string pixKey, decimal amount)
    {
        var saga = new PixSaga(accountId, amount);
        // Nota: O PixKey deveria estar no Saga. Vamos atualizar a entidade PixSaga depois se necessário.

        using var uow = await _txManager.BeginTransactionAsync();

        await _sagaRepo.AddAsync(saga);

        // Dispara o primeiro passo da Saga
        await _orchestrator.ProcessPixSaga(saga.Id);

        await uow.CommitAsync();

        return saga.Id;
    }
}