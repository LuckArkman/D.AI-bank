using Fintech.Entities;
using Fintech.Interfaces;
using Fintech.Persistence;
using MongoDB.Driver;

using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace Fintech.Repositories;

public class TenantRepository : ITenantRepository
{
    private readonly IMongoCollection<Tenant> _collection;
    private readonly IDistributedCache _cache;

    public TenantRepository(MongoContext context, IDistributedCache cache)
    {
        _collection = context.Database.GetCollection<Tenant>("tenants");
        _cache = cache;
    }

    public async Task AddAsync(Tenant tenant) => await _collection.InsertOneAsync(tenant);

    public async Task<Tenant?> GetByIdAsync(Guid id)
    {
        var cacheKey = $"tenant:id:{id}";
        var cached = await _cache.GetStringAsync(cacheKey);
        if (!string.IsNullOrEmpty(cached))
        {
            return JsonSerializer.Deserialize<Tenant>(cached);
        }

        var tenant = await _collection.Find(x => x.Id == id).FirstOrDefaultAsync();
        if (tenant != null)
        {
            await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(tenant),
                new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10) });
        }
        return tenant;
    }

    public async Task<Tenant?> GetByIdentifierAsync(string identifier)
    {
        var cacheKey = $"tenant:identifier:{identifier}";
        var cached = await _cache.GetStringAsync(cacheKey);
        if (!string.IsNullOrEmpty(cached))
        {
            return JsonSerializer.Deserialize<Tenant>(cached);
        }

        var tenant = await _collection.Find(x => x.Identifier == identifier).FirstOrDefaultAsync();
        if (tenant != null)
        {
            await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(tenant),
                new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10) });
        }
        return tenant;
    }

    public async Task<IEnumerable<Tenant>> GetAllAsync() =>
        await _collection.Find(_ => true).ToListAsync();

    public async Task UpdateAsync(Tenant tenant)
    {
        await _collection.ReplaceOneAsync(x => x.Id == tenant.Id, tenant);

        // Invalidate cache
        await _cache.RemoveAsync($"tenant:id:{tenant.Id}");
        await _cache.RemoveAsync($"tenant:identifier:{tenant.Identifier}");
    }
}
