using MongoDB.Driver;
using Fintech.Entities;
using Fintech.Persistence;

namespace Fintech.Repositories;

public class LedgerRepository
{
    private readonly MongoContext _context;
    private readonly IMongoCollection<LedgerEvent> _collection;

    public LedgerRepository(MongoContext context)
    {
        _context = context;
        _collection = _context.Database.GetCollection<LedgerEvent>("ledger");
    }

    public async Task AddAsync(LedgerEvent ledgerEvent)
    {
        if (_context.Session != null)
        {
            await _collection.InsertOneAsync(_context.Session, ledgerEvent);
        }
        else
        {
            await _collection.InsertOneAsync(ledgerEvent);
        }
    }
}