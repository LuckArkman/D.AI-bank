using Fintech.Entities;
using MongoDB.Driver;

namespace Fintech.Commands;

public class DebitAccountHandler
{
    private readonly IMongoClient _client;
    private readonly IMongoCollection<Account> _accounts;
    private readonly IMongoCollection<LedgerEvent> _ledger;
    private readonly IMongoCollection<OutboxMessage> _outbox;

    public DebitAccountHandler(IMongoClient client)
    {
        _client = client;
        var db = _client.GetDatabase("FintechDB");
        _accounts = db.GetCollection<Account>("accounts");
        _ledger = db.GetCollection<LedgerEvent>("ledger");
        _outbox = db.GetCollection<OutboxMessage>("outbox");
    }

    public async Task HandleAsync(Guid accountId, decimal amount, Guid correlationId)
    {
        // 1. Iniciar Sessão para Transação ACID (Multi-Document Transaction)
        using var session = await _client.StartSessionAsync();
        session.StartTransaction();

        try
        {
            // 2. Carregar a conta (Estado atual)
            // Nota: Em cenários extremos, usamos findOneAndUpdate para lockar, 
            // mas aqui usaremos Optimistic Concurrency na escrita.
            var account = await _accounts.Find(session, a => a.Id == accountId)
                                         .FirstOrDefaultAsync();

            if (account == null) throw new Exception("Conta não encontrada");

            // 3. Validação de Negócio (Strong Consistency)
            if (account.AvailableBalance < amount)
            {
                throw new InvalidOperationException($"Saldo insuficiente. Disponível: {account.AvailableBalance}");
            }

            // 4. Preparar os dados
            var newBalance = account.AvailableBalance - amount;
            var newVersion = account.Version + 1;

            var debitEvent = new LedgerEvent
            {
                AccountId = accountId,
                Type = "DEBIT",
                Amount = amount,
                BalanceAfter = newBalance,
                CorrelationId = correlationId,
                Metadata = new Dictionary<string, string> { { "Reason", "Payment" } }
            };

            var outboxMessage = new OutboxMessage
            {
                Topic = "account-events",
                PayloadJson = System.Text.Json.JsonSerializer.Serialize(debitEvent)
            };

            // 5. Executar Escrita Atômica no Estado (State Store)
            // AQUI ESTÁ A MÁGICA DO OPTIMISTIC LOCKING
            var filter = Builders<Account>.Filter.And(
                Builders<Account>.Filter.Eq(a => a.Id, accountId),
                Builders<Account>.Filter.Eq(a => a.Version, account.Version) // Garante que ninguém mexeu
            );

            var update = Builders<Account>.Update
                .Set(a => a.AvailableBalance, newBalance)
                .Set(a => a.Version, newVersion)
                .Set(a => a.LastUpdated, DateTime.UtcNow);

            var result = await _accounts.UpdateOneAsync(session, filter, update);

            if (result.ModifiedCount == 0)
            {
                // Se chegou aqui, outra thread alterou o saldo milissegundos antes.
                // Abortamos tudo e o cliente (ou o Polly) deve tentar novamente.
                throw new ConcurrencyException("Conflito de concorrência. Tente novamente.");
            }

            // 6. Persistir Ledger e Outbox (Só acontece se o UpdateOne passou)
            await _ledger.InsertOneAsync(session, debitEvent);
            await _outbox.InsertOneAsync(session, outboxMessage);

            // 7. Commit (Tudo ou nada)
            await session.CommitTransactionAsync();
        }
        catch (Exception)
        {
            await session.AbortTransactionAsync();
            throw; // Repassa o erro para o middleware de tratamento global
        }
    }
}