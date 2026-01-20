using Fintech.Entities;

namespace Fintech.Interfaces;

public interface ICardRepository
{
    Task AddAsync(AccountCard card);
    Task<IEnumerable<AccountCard>> GetByAccountIdAsync(Guid accountId);
    Task<AccountCard> GetByIdAsync(Guid cardId);
    Task UpdateAsync(AccountCard card);
}
