using MongoDB.Driver;
using Fintech.Entities;
using Fintech.Persistence;
using Fintech.Core.Interfaces;
using Fintech.Enums;
using Fintech.Interfaces;

namespace Fintech.Repositories;

public class SagaRepository : ISagaRepository
{
    private readonly MongoContext _context;
    private readonly IMongoCollection<PixSaga> _collection;
    private readonly ITenantProvider _tenantProvider;

    public SagaRepository(MongoContext context, ITenantProvider tenantProvider)
    {
        _context = context;
        _collection = _context.Database.GetCollection<PixSaga>("sagas");
        _tenantProvider = tenantProvider;
    }

    public async Task AddAsync(PixSaga saga)
    {
        if (_context.Session != null) await _collection.InsertOneAsync(_context.Session, saga);
        else await _collection.InsertOneAsync(saga);
    }

    public async Task<PixSaga> GetByIdAsync(Guid id)
    {
        return await _collection.Find(x => x.Id == id && x.TenantId == _tenantProvider.TenantId).FirstOrDefaultAsync();
    }

    public async Task UpdateAsync(PixSaga saga)
    {
        var filter = Builders<PixSaga>.Filter.And(
            Builders<PixSaga>.Filter.Eq(x => x.Id, saga.Id),
            Builders<PixSaga>.Filter.Eq(x => x.TenantId, _tenantProvider.TenantId)
        );

        if (_context.Session != null)
            await _collection.ReplaceOneAsync(_context.Session, filter, saga);
        else
            await _collection.ReplaceOneAsync(filter, saga);
    }

    public async Task<IEnumerable<PixSaga>> GetPendingAsync()
    {
        return await _collection.Find(x => x.Status == PixStatus.Created || x.Status == PixStatus.BalanceLocked).ToListAsync();
    }
}
