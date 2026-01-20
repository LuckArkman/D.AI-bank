using MongoDB.Driver;
using Fintech.Core.Entities;
using Fintech.Core.Interfaces;
using Fintech.Entities;
using Fintech.Persistence;

namespace Fintech.Repositories;

public class PixKeyRepository : IPixKeyRepository
{
    private readonly MongoContext _context;
    private readonly IMongoCollection<PixKey> _collection;

    public PixKeyRepository(MongoContext context)
    {
        _context = context;
        _collection = _context.Database.GetCollection<PixKey>("pix_keys");
    }

    public async Task AddAsync(PixKey pixKey)
    {
        if (_context.Session != null)
        {
            await _collection.InsertOneAsync(_context.Session, pixKey);
        }
        else
        {
            await _collection.InsertOneAsync(pixKey);
        }
    }

    public async Task<PixKey?> GetByKeyAsync(string key)
    {
        var filter = Builders<PixKey>.Filter.Eq(x => x.Key, key);

        if (_context.Session != null)
        {
            return await _collection.Find(_context.Session, filter).FirstOrDefaultAsync();
        }

        return await _collection.Find(filter).FirstOrDefaultAsync();
    }

    public async Task<bool> ExistsAsync(string key)
    {
        var filter = Builders<PixKey>.Filter.Eq(x => x.Key, key);

        if (_context.Session != null)
        {
            return await _collection.Find(_context.Session, filter).AnyAsync();
        }

        return await _collection.Find(filter).AnyAsync();
    }

    public async Task<List<PixKey>> GetByAccountIdAsync(Guid accountId)
    {
        var filter = Builders<PixKey>.Filter.Eq(x => x.AccountId, accountId);

        if (_context.Session != null)
        {
            return await _collection.Find(_context.Session, filter).ToListAsync();
        }

        return await _collection.Find(filter).ToListAsync();
    }
}