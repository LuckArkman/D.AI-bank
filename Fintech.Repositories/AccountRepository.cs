using Fintech.Core.Interfaces;
using MongoDB.Driver;
using Fintech.Entities;
using Fintech.Interfaces;
using Fintech.Exceptions;
using Fintech.Entities;
using Fintech.Exceptions;
using Fintech.Persistence; // Assumindo que você criou as exceptions customizadas

namespace Fintech.Repositories;

public class AccountRepository : IAccountRepository
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
        var filter = Builders<Account>.Filter.Eq(x => x.Id, id);
        
        // Se houver uma transação ativa no contexto, usamos ela.
        // Isso garante "Read Your Own Writes" dentro da transação.
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
        // Regra de segurança: Updates de saldo DEVEM ocorrer dentro de transação/UoW
        if (_context.Session == null)
        {
            throw new InvalidOperationException("Operações de escrita em Conta exigem uma Transação ativa (UnitOfWork).");
        }

        // --- OPTIMISTIC CONCURRENCY CONTROL (OCC) ---
        // A query de update procura pelo ID E pela Versão que foi lida da memória.
        var filter = Builders<Account>.Filter.And(
            Builders<Account>.Filter.Eq(x => x.Id, account.Id),
            Builders<Account>.Filter.Eq(x => x.Version, account.Version)
        );

        // Preparamos o update:
        // 1. Atualiza os saldos (Value Object Money já serializado corretamente)
        // 2. Atualiza data
        // 3. Incrementa a versão (+1) atomicamente no banco
        var update = Builders<Account>.Update
            .Set(x => x.Balances, account.Balances)
            .Set(x => x.LastUpdated, DateTime.UtcNow)
            .Inc(x => x.Version, 1);

        var result = await _collection.UpdateOneAsync(_context.Session, filter, update);

        // Se ModifiedCount for 0, significa que:
        // A) A conta não existe mais.
        // B) A versão no banco mudou desde que lemos (Race Condition).
        if (result.ModifiedCount == 0)
        {
            throw new ConcurrencyException("Falha de concorrência: O registro foi modificado por outra transação antes da conclusão desta.");
        }
        
        // Nota: A entidade em memória 'account' fica com a Version desatualizada (N-1) 
        // em relação ao banco (N) neste momento, mas isso é aceitável pois a transação vai acabar.
    }
}