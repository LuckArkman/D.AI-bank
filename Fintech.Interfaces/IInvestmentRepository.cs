using Fintech.Entities;

namespace Fintech.Interfaces;

public interface IInvestmentRepository
{
    Task AddInvestmentAsync(Investment investment);
    Task<IEnumerable<Investment>> GetInvestmentsByAccountAsync(Guid accountId);
    Task UpdateInvestmentAsync(Investment investment);

    Task AddGoalAsync(SavingsGoal goal);
    Task<IEnumerable<SavingsGoal>> GetGoalsByAccountAsync(Guid accountId);
    Task<SavingsGoal> GetGoalByIdAsync(Guid goalId);
    Task UpdateGoalAsync(SavingsGoal goal);
}
