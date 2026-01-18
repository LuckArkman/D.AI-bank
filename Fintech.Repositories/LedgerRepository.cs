using MongoDB.Driver;
using Fintech.Core.Entities;
using Fintech.Core.Interfaces;
using Fintech.Entities;
using Fintech.Persistence;

namespace Fintech.Repositories;

public class LedgerRepository : ILedgerRepository
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
        // O Ledger é Append-Only (Apenas inserção).
        // Se houver sessão, participa da transação ACID (Atomicidade com o Saldo).
        
        if (_context.Session != null)
        {
            await _collection.InsertOneAsync(_context.Session, ledgerEvent);
        }
        else
        {
            // Em cenários raros (ex: log de erro fora de tx), permite insert direto.
            await _collection.InsertOneAsync(ledgerEvent);
        }
    }
}