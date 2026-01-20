using Fintech.Entities;

namespace Fintech.Interfaces;

public interface ITenantRepository
{
    Task AddAsync(Tenant tenant);
    Task<Tenant?> GetByIdAsync(Guid id);
    Task<Tenant?> GetByIdentifierAsync(string identifier);
    Task<IEnumerable<Tenant>> GetAllAsync();
    Task UpdateAsync(Tenant tenant);
}
