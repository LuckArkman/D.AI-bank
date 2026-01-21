using Fintech.Entities;

namespace Fintech.Core.Interfaces;

public interface ISagaRepository
{
    Task AddAsync(PixSaga saga);
    Task<PixSaga> GetByIdAsync(Guid id);
    Task UpdateAsync(PixSaga saga);
    Task<IEnumerable<PixSaga>> GetPendingAsync();
}
