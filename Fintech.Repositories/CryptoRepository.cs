using Fintech.Entities;
using Fintech.Interfaces;
using Fintech.Persistence;
using MongoDB.Driver;

namespace Fintech.Repositories;

public class CryptoRepository : ICryptoRepository
{
    private readonly IMongoCollection<CryptoAsset> _collection;
    private readonly ITenantProvider _tenantProvider;

    public CryptoRepository(MongoContext context, ITenantProvider tenantProvider)
    {
        _collection = context.Database.GetCollection<CryptoAsset>("crypto_assets");
        _tenantProvider = tenantProvider;
    }

    public async Task<CryptoAsset?> GetByAccountAndSymbolAsync(Guid accountId, string symbol)
    {
        var tenantId = _tenantProvider.TenantId;
        return await _collection.Find(x => x.AccountId == accountId && x.Symbol == symbol && x.TenantId == tenantId).FirstOrDefaultAsync();
    }

    public async Task<List<CryptoAsset>> GetByAccountIdAsync(Guid accountId)
    {
        var tenantId = _tenantProvider.TenantId;
        return await _collection.Find(x => x.AccountId == accountId && x.TenantId == tenantId).ToListAsync();
    }

    public async Task AddAsync(CryptoAsset asset)
    {
        asset.TenantId = _tenantProvider.TenantId ?? throw new Exception("TenantId is required");
        await _collection.InsertOneAsync(asset);
    }

    public async Task UpdateAsync(CryptoAsset asset)
    {
        var filter = Builders<CryptoAsset>.Filter.Eq(x => x.Id, asset.Id);
        await _collection.ReplaceOneAsync(filter, asset);
    }
}
