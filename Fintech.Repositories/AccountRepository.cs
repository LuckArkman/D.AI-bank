using Fintech.Core.Interfaces;
using MongoDB.Driver;
using Fintech.Entities;
using Fintech.Interfaces;
using Fintech.Exceptions;
using Fintech.Persistence; // Assumindo que você criou as exceptions customizadas


namespace Fintech.Repositories;

public class AccountRepository : IAccountRepository
{
    private readonly MongoContext _context;
    private readonly ITenantProvider _tenantProvider;
    private readonly IMongoCollection<Account> _collection;

    public AccountRepository(MongoContext context, ITenantProvider tenantProvider)
    {
        _context = context;
        _tenantProvider = tenantProvider;
        _collection = _context.Database.GetCollection<Account>("accounts");
    }

    public async Task<Account> GetByIdAsync(Guid id)
    {
        var filter = Builders<Account>.Filter.And(
            Builders<Account>.Filter.Eq(x => x.Id, id),
            Builders<Account>.Filter.Eq(x => x.TenantId, _tenantProvider.TenantId)
        );
        var account = _context.Session != null
            ? await _collection.Find(_context.Session, filter).FirstOrDefaultAsync()
            : await _collection.Find(filter).FirstOrDefaultAsync();

        if (account == null)
        {
            throw new KeyNotFoundException($"Conta {id} não encontrada.");
        }

        return account;
    }

    public async Task AddAsync(Account account)
    {
        if (_context.Session != null)
        {
            await _collection.InsertOneAsync(_context.Session, account);
        }
        else
        {
            await _collection.InsertOneAsync(account);
        }
    }

    public async Task UpdateAsync(Account account)
    {
        if (_context.Session == null)
        {
            throw new InvalidOperationException("Operações de escrita em Conta exigem uma Transação ativa (UnitOfWork).");
        }
        var filter = Builders<Account>.Filter.And(
            Builders<Account>.Filter.Eq(x => x.Id, account.Id),
            Builders<Account>.Filter.Eq(x => x.TenantId, _tenantProvider.TenantId),
            Builders<Account>.Filter.Eq(x => x.Version, account.Version)
        );
        var update = Builders<Account>.Update
            .Set(x => x.Balances, account.Balances)
            .Set(x => x.LastUpdated, DateTime.UtcNow)
            .Inc(x => x.Version, 1);

        var result = await _collection.UpdateOneAsync(_context.Session, filter, update);
        if (result.ModifiedCount == 0)
        {
            throw new ConcurrencyException("Falha de concorrência: O registro foi modificado por outra transação antes da conclusão desta.");
        }
    }

    public async Task<IEnumerable<Account>> GetAllAsync()
    {
        var filter = Builders<Account>.Filter.Eq(x => x.TenantId, _tenantProvider.TenantId);
        return await _collection.Find(filter).ToListAsync();
    }
}
