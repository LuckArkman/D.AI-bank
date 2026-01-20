using Fintech.Entities;

namespace Fintech.Interfaces;

public interface IOutboxRepository
{
    Task AddAsync(OutboxMessage message);
    Task<List<OutboxMessage>> GetPendingAsync(int limit);
    Task MarkAsProcessedAsync(Guid id);
}