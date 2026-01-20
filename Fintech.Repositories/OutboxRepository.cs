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
        var update = Builders<OutboxMessage>.Update.Set(x => x.ProcessedAt, DateTime.UtcNow);
        await _collection.UpdateOneAsync(x => x.Id == id, update);
    }
}