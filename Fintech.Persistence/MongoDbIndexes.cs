using Fintech.Core.Entities;
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

        // Accounts - Add TenantId index
        await accountsCollection.Indexes.CreateOneAsync(
            new CreateIndexModel<Account>(
                Builders<Account>.IndexKeys.Ascending(x => x.TenantId)
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
            ),
            new CreateIndexModel<LedgerEvent>(
                Builders<LedgerEvent>.IndexKeys.Ascending(x => x.TenantId)
            )
        });

        // Rules
        var rulesCollection = db.GetCollection<BusinessRule>("business_rules");
        await rulesCollection.Indexes.CreateOneAsync(
            new CreateIndexModel<BusinessRule>(
                Builders<BusinessRule>.IndexKeys.Ascending(x => x.TenantId)
            )
        );

        // Users
        var usersCollection = db.GetCollection<User>("users");
        await usersCollection.Indexes.CreateOneAsync(
            new CreateIndexModel<User>(
                Builders<User>.IndexKeys.Ascending(x => x.TenantId)
            )
        );

        // Crypto Assets
        var cryptoCollection = db.GetCollection<CryptoAsset>("crypto_assets");
        await cryptoCollection.Indexes.CreateOneAsync(
            new CreateIndexModel<CryptoAsset>(
                Builders<CryptoAsset>.IndexKeys.Ascending(x => x.AccountId).Ascending(x => x.Symbol)
            )
        );
        // Liquidity Pools
        var liquidityCollection = db.GetCollection<LiquidityPool>("liquidity_pools");
        await liquidityCollection.Indexes.CreateOneAsync(
            new CreateIndexModel<LiquidityPool>(
                Builders<LiquidityPool>.IndexKeys.Ascending(x => x.Network).Ascending(x => x.CurrencyCode),
                new CreateIndexOptions { Unique = true }
            )
        );
    }
}
