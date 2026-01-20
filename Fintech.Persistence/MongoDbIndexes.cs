using Fintech.Entities;
using MongoDB.Driver;

namespace Fintech.Persistence;

public static class MongoDbIndexes
{
    public static async Task CreateIndexes(IMongoDatabase db)
    {
        // Accounts
        var accountsCollection = db.GetCollection<Account>("accounts");
        await accountsCollection.Indexes.CreateOneAsync(
            new CreateIndexModel<Account>(
                Builders<Account>.IndexKeys.Ascending(x => x.Id),
                new CreateIndexOptions { Unique = true }
            )
        );

        // PixKeys
        var pixKeysCollection = db.GetCollection<PixKey>("pixkeys");
        await pixKeysCollection.Indexes.CreateOneAsync(
            new CreateIndexModel<PixKey>(
                Builders<PixKey>.IndexKeys.Ascending(x => x.Key),
                new CreateIndexOptions { Unique = true }
            )
        );

        // Outbox
        var outboxCollection = db.GetCollection<OutboxMessage>("outbox");
        await outboxCollection.Indexes.CreateOneAsync(
            new CreateIndexModel<OutboxMessage>(
                Builders<OutboxMessage>.IndexKeys
                    .Ascending(x => x.ProcessedAt)
                    .Ascending(x => x.CreatedAt)
            )
        );

        // Ledger
        var ledgerCollection = db.GetCollection<LedgerEvent>("ledger");
        await ledgerCollection.Indexes.CreateManyAsync(new[]
        {
            new CreateIndexModel<LedgerEvent>(
                Builders<LedgerEvent>.IndexKeys.Ascending(x => x.AccountId)
            ),
            new CreateIndexModel<LedgerEvent>(
                Builders<LedgerEvent>.IndexKeys.Descending(x => x.Timestamp)
            ),
            new CreateIndexModel<LedgerEvent>(
                Builders<LedgerEvent>.IndexKeys.Ascending(x => x.CorrelationId)
            )
        });
    }
}
