using Fintech.Entities;
using Fintech.Interfaces;
using Fintech.Persistence;
using MongoDB.Driver;

namespace Fintech.Repositories;

public class TenantRepository : ITenantRepository
{
    private readonly IMongoCollection<Tenant> _collection;

    public TenantRepository(MongoContext context)
    {
        _collection = context.Database.GetCollection<Tenant>("tenants");
    }

    public async Task AddAsync(Tenant tenant) => await _collection.InsertOneAsync(tenant);

    public async Task<Tenant?> GetByIdAsync(Guid id) =>
        await _collection.Find(x => x.Id == id).FirstOrDefaultAsync();

    public async Task<Tenant?> GetByIdentifierAsync(string identifier) =>
        await _collection.Find(x => x.Identifier == identifier).FirstOrDefaultAsync();

    public async Task<IEnumerable<Tenant>> GetAllAsync() =>
        await _collection.Find(_ => true).ToListAsync();

    public async Task UpdateAsync(Tenant tenant) =>
        await _collection.ReplaceOneAsync(x => x.Id == tenant.Id, tenant);
}
