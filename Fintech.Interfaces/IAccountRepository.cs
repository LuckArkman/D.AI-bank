using Fintech.Core.Entities;
using Fintech.Entities;

namespace Fintech.Core.Interfaces;

public interface IAccountRepository
{
    Task<Account> GetByIdAsync(Guid id);
    Task AddAsync(Account account);
    Task UpdateAsync(Account account);
}