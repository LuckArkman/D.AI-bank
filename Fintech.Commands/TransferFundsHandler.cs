using Fintech.Entities;
using Fintech.Exceptions;
using Fintech.Services;
using MongoDB.Driver;

namespace Fintech.Commands;

public class TransferFundsHandler
{
    private readonly IMongoClient _client;
    private readonly IMongoCollection<Account> _accounts;
    private readonly IMongoCollection<LedgerEvent> _ledger;
    private readonly IdempotencyService _idempotency;

    public TransferFundsHandler(IMongoClient client, IdempotencyService idempotency)
    {
        _client = client;
        _idempotency = idempotency;
        var db = _client.GetDatabase("FintechDB");
        _accounts = db.GetCollection<Account>("accounts");
        _ledger = db.GetCollection<LedgerEvent>("ledger");
    }

    public async Task HandleAsync(Guid fromAccountId, Guid toAccountId, decimal amount, Guid commandId)
    {
        using var session = await _client.StartSessionAsync();
        session.StartTransaction();

        try
        {
            // 1. Check de Idempotência
            var existing = await _idempotency.TryLockAsync(commandId, session);
            if (existing != null) return; // Já processado, retorna ok silenciosamente ou lança exception específica

            // 2. Carregar as duas contas
            // Dica: Use Task.WhenAll para performance, mas cuidado com Deadlocks em updates cruzados.
            // Para segurança máxima, ordene os IDs para evitar Deadlock (ex: sempre lockar o menor ID primeiro).
            var accFrom = await _accounts.Find(session, x => x.Id == fromAccountId).FirstOrDefaultAsync();
            var accTo = await _accounts.Find(session, x => x.Id == toAccountId).FirstOrDefaultAsync();

            if (accFrom == null || accTo == null) throw new Exception("Conta inválida");
            if (accFrom.AvailableBalance < amount) throw new InvalidOperationException("Saldo insuficiente");

            // 3. Preparar Updates (Optimistic Concurrency)
            var updateFrom = Builders<Account>.Update
                .Inc(x => x.AvailableBalance, -amount)
                .Inc(x => x.Version, 1)
                .Set(x => x.LastUpdated, DateTime.UtcNow);

            var updateTo = Builders<Account>.Update
                .Inc(x => x.AvailableBalance, amount)
                .Inc(x => x.Version, 1)
                .Set(x => x.LastUpdated, DateTime.UtcNow);

            // 4. Executar Débito
            var resFrom = await _accounts.UpdateOneAsync(session, 
                x => x.Id == fromAccountId && x.Version == accFrom.Version, updateFrom);
            
            if (resFrom.ModifiedCount == 0) throw new ConcurrencyException("Falha no débito (concorrência)");

            // 5. Executar Crédito
            var resTo = await _accounts.UpdateOneAsync(session, 
                x => x.Id == toAccountId && x.Version == accTo.Version, updateTo);

            if (resTo.ModifiedCount == 0) throw new ConcurrencyException("Falha no crédito (concorrência)");

            // 6. Gravar Ledger (Dupla entrada)
            var debitEvent = CreateLedgerEvent(fromAccountId, "TRANSFER_SENT", -amount, commandId);
            var creditEvent = CreateLedgerEvent(toAccountId, "TRANSFER_RECEIVED", amount, commandId);

            await _ledger.InsertManyAsync(session, new[] { debitEvent, creditEvent });

            // 7. Finalizar Idempotência e Commit
            await _idempotency.CompleteAsync(commandId, true, new { Status = "Transferido" }, session);
            await session.CommitTransactionAsync();
        }
        catch
        {
            await session.AbortTransactionAsync();
            throw;
        }
    }

    private LedgerEvent CreateLedgerEvent(Guid accId, string type, decimal amount, Guid corrId)
    {
        return new LedgerEvent 
        { 
            AccountId = accId, 
            Type = type, 
            Amount = amount, 
            CorrelationId = corrId,
            Timestamp = DateTime.UtcNow
        };
    }
}