using Fintech.Entities;

namespace Fintech.Interfaces;

public interface IOutboxRepository
{
    Task AddAsync(OutboxMessage message);
    Task<List<OutboxMessage>> GetPendingAsync(int limit);
    Task<List<OutboxMessage>> LockAndGetAsync(int limit, string workerId);
    Task MarkAsProcessedAsync(Guid id);
    Task UnlockAsync(Guid id);
}
