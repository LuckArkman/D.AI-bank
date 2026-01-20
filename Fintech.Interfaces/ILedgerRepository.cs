using Fintech.Core.Entities;
using Fintech.Entities;

namespace Fintech.Core.Interfaces;

public interface ILedgerRepository
{
    Task AddAsync(LedgerEvent ledgerEvent);
    Task<IEnumerable<LedgerEvent>> GetAllAsync();
    Task<IEnumerable<LedgerEvent>> GetByAccountIdAsync(Guid accountId);
}
