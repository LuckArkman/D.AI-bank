using Fintech.Entities;
using Fintech.Interfaces;
using Fintech.Persistence;
using MongoDB.Driver;

namespace Fintech.Repositories;

public class CardRepository : ICardRepository
{
    private readonly IMongoCollection<AccountCard> _collection;

    public CardRepository(MongoContext context)
    {
        _collection = context.Database.GetCollection<AccountCard>("cards");
    }

    public async Task AddAsync(AccountCard card) => await _collection.InsertOneAsync(card);

    public async Task<IEnumerable<AccountCard>> GetByAccountIdAsync(Guid accountId)
    {
        return await _collection.Find(x => x.AccountId == accountId).ToListAsync();
    }

    public async Task<AccountCard> GetByIdAsync(Guid cardId)
    {
        return await _collection.Find(x => x.Id == cardId).FirstOrDefaultAsync();
    }

    public async Task UpdateAsync(AccountCard card)
    {
        await _collection.ReplaceOneAsync(x => x.Id == card.Id, card);
    }
}
