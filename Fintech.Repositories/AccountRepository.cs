using MongoDB.Driver;
using Fintech.Entities;
using Fintech.Exceptions;
using Fintech.Persistence;

namespace Fintech.Repositories;

public class AccountRepository
{
    private readonly MongoContext _context;
    private readonly IMongoCollection<Account> _collection;

    public AccountRepository(MongoContext context)
    {
        _context = context;
        _collection = _context.Database.GetCollection<Account>("accounts");
    }

    public async Task<Account> GetByIdAsync(Guid id)
    {
        // Se houver transação ativa, usa a sessão. Se não, vai direto.
        var filter = Builders<Account>.Filter.Eq(x => x.Id, id);
        
        var entity = _context.Session != null 
            ? await _collection.Find(_context.Session, filter).FirstOrDefaultAsync()
            : await _collection.Find(filter).FirstOrDefaultAsync();

        if (entity == null) throw new KeyNotFoundException($"Conta {id} não encontrada.");
        return entity;
    }

    public async Task UpdateAsync(Account account)
    {
        if (_context.Session == null) 
            throw new InvalidOperationException("Operações de escrita em Conta exigem uma Transação ativa.");

        // O GRANDE SEGREDO: Optimistic Locking
        // Filtra pelo ID **E** pela Versão que lemos anteriormente
        var filter = Builders<Account>.Filter.And(
            Builders<Account>.Filter.Eq(x => x.Id, account.Id),
            Builders<Account>.Filter.Eq(x => x.Version, account.Version)
        );

        // Prepara o update incrementando a versão
        var update = Builders<Account>.Update
            .Set(x => x.Balances, account.Balances)
            .Set(x => x.LastUpdated, DateTime.UtcNow)
            .Inc(x => x.Version, 1);

        var result = await _collection.UpdateOneAsync(_context.Session, filter, update);

        if (result.ModifiedCount == 0)
        {
            throw new InvalidOperationException("Falha de Concorrência: A conta foi modificada por outra transação.");
        }
    }

    public async Task AddAsync(Account account)
    {
        if (_context.Session != null)
            await _collection.InsertOneAsync(_context.Session, account);
        else
            await _collection.InsertOneAsync(account);
    }
}