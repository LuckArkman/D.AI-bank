using Fintech.Entities;

namespace Fintech.Interfaces;

public interface IRuleRepository
{
    Task<BusinessRule> GetByIdAsync(string id);
    Task<List<BusinessRule>> GetAllAsync();
    Task AddAsync(BusinessRule rule);
    Task UpdateAsync(BusinessRule rule);
    Task DeleteAsync(string id);
}
