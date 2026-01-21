using MongoDB.Driver;
using Fintech.Entities;
using Fintech.Interfaces;
using Fintech.Persistence;

namespace Fintech.Repositories;

public class OutboxRepository : IOutboxRepository
{
    private readonly MongoContext _context;
    private readonly IMongoCollection<OutboxMessage> _collection;

    public OutboxRepository(MongoContext context)
    {
        _context = context;
        _collection = _context.Database.GetCollection<OutboxMessage>("outbox");
    }

    public async Task AddAsync(OutboxMessage message)
    {
        // Obrigatório estar na transação da conta
        if (_context.Session == null) throw new InvalidOperationException("Outbox exige transação.");
        await _collection.InsertOneAsync(_context.Session, message);
    }

    public async Task<List<OutboxMessage>> GetPendingAsync(int limit)
    {
        // Busca mensagens onde ProcessedAt é nulo
        // Ordena por CreatedAt para manter FIFO (dentro do possível)
        return await _collection.Find(x => x.ProcessedAt == null)
            .SortBy(x => x.CreatedAt)
            .Limit(limit)
            .ToListAsync();
    }

    public async Task MarkAsProcessedAsync(Guid id)
    {
        var update = Builders<OutboxMessage>.Update
            .Set(x => x.ProcessedAt, DateTime.UtcNow)
            .Set(x => x.Processed, true);
        await _collection.UpdateOneAsync(x => x.Id == id, update);
    }

    public async Task<List<OutboxMessage>> LockAndGetAsync(int limit, string workerId)
    {
        var timeout = DateTime.UtcNow.AddMinutes(-5);
        var filter = Builders<OutboxMessage>.Filter.And(
            Builders<OutboxMessage>.Filter.Eq(x => x.ProcessedAt, null),
            Builders<OutboxMessage>.Filter.Or(
                Builders<OutboxMessage>.Filter.Eq(x => x.LockedAt, null),
                Builders<OutboxMessage>.Filter.Lt(x => x.LockedAt, timeout)
            )
        );

        var update = Builders<OutboxMessage>.Update
            .Set(x => x.LockedBy, workerId)
            .Set(x => x.LockedAt, DateTime.UtcNow);

        var messages = new List<OutboxMessage>();
        for (int i = 0; i < limit; i++)
        {
            // FindOneAndUpdate é atômico no MongoDB
            var msg = await _collection.FindOneAndUpdateAsync(filter, update);
            if (msg == null) break;
            messages.Add(msg);
        }
        return messages;
    }

    public async Task UnlockAsync(Guid id)
    {
        var update = Builders<OutboxMessage>.Update
            .Set(x => x.LockedBy, null)
            .Set(x => x.LockedAt, null);
        await _collection.UpdateOneAsync(x => x.Id == id, update);
    }
}
