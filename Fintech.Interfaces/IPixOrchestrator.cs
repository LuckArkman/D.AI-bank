namespace Fintech.Interfaces;

public interface IPixOrchestrator
{
    Task ProcessPixSaga(Guid sagaId);
}