using MongoDB.Driver;
using Fintech.Entities;
using Fintech.Persistence;

namespace Fintech.Repositories;

public class SagaRepository
{
    private readonly MongoContext _context;
    private readonly IMongoCollection<PixSaga> _collection;

    public SagaRepository(MongoContext context)
    {
        _context = context;
        _collection = _context.Database.GetCollection<PixSaga>("sagas");
    }

    public async Task AddAsync(PixSaga saga)
    {
        if (_context.Session != null) await _collection.InsertOneAsync(_context.Session, saga);
        else await _collection.InsertOneAsync(saga);
    }

    public async Task<PixSaga> GetByIdAsync(Guid id)
    {
        return await _collection.Find(x => x.Id == id).FirstOrDefaultAsync();
    }

    public async Task UpdateAsync(PixSaga saga)
    {
        // Sagas geralmente não precisam de Optimistic Locking agressivo
        // pois são orquestradas por eventos sequenciais, mas é boa prática ter.
        var filter = Builders<PixSaga>.Filter.Eq(x => x.Id, saga.Id);
        
        if (_context.Session != null)
            await _collection.ReplaceOneAsync(_context.Session, filter, saga);
        else
            await _collection.ReplaceOneAsync(filter, saga);
    }
}