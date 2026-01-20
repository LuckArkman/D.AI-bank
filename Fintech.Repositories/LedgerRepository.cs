using MongoDB.Driver;
using Fintech.Core.Entities;
using Fintech.Core.Interfaces;
using Fintech.Entities;
using Fintech.Persistence;
using Fintech.Interfaces;

namespace Fintech.Repositories;

public class LedgerRepository : ILedgerRepository
{
    private readonly MongoContext _context;
    private readonly IMongoCollection<LedgerEvent> _collection;
    private readonly ITenantProvider _tenantProvider;

    public LedgerRepository(MongoContext context, ITenantProvider tenantProvider)
    {
        _context = context;
        _collection = _context.Database.GetCollection<LedgerEvent>("ledger");
        _tenantProvider = tenantProvider;
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

    public async Task<IEnumerable<LedgerEvent>> GetAllAsync()
    {
        return await _collection.Find(x => x.TenantId == _tenantProvider.TenantId).ToListAsync();
    }

    public async Task<IEnumerable<LedgerEvent>> GetByAccountIdAsync(Guid accountId)
    {
        return await _collection.Find(e => e.AccountId == accountId && e.TenantId == _tenantProvider.TenantId).ToListAsync();
    }
}
