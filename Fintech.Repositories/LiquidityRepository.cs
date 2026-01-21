using Fintech.Entities;
using Fintech.Interfaces;
using Fintech.Persistence;
using MongoDB.Driver;

namespace Fintech.Repositories;

public class LiquidityRepository : ILiquidityRepository
{
    private readonly IMongoCollection<LiquidityPool> _collection;

    public LiquidityRepository(MongoContext context)
    {
        _collection = context.Database.GetCollection<LiquidityPool>("liquidity_pools");
    }

    public async Task<LiquidityPool?> GetByNetworkAndCurrencyAsync(string network, string currencyCode)
    {
        return await _collection.Find(p => p.Network == network && p.CurrencyCode == currencyCode).FirstOrDefaultAsync();
    }

    public async Task UpdateAsync(LiquidityPool pool)
    {
        await _collection.ReplaceOneAsync(p => p.Id == pool.Id, pool);
    }

    public async Task AddAsync(LiquidityPool pool)
    {
        await _collection.InsertOneAsync(pool);
    }

    public async Task<IEnumerable<LiquidityPool>> GetAllAsync()
    {
        return await _collection.Find(_ => true).ToListAsync();
    }
}
