using Fintech.Entities;
using Fintech.Interfaces;
using Fintech.Persistence;
using MongoDB.Driver;

namespace Fintech.Repositories;

public class InvestmentRepository : IInvestmentRepository
{
    private readonly IMongoCollection<Investment> _investments;
    private readonly IMongoCollection<SavingsGoal> _goals;

    public InvestmentRepository(MongoContext context)
    {
        _investments = context.Database.GetCollection<Investment>("investments");
        _goals = context.Database.GetCollection<SavingsGoal>("goals");
    }

    public async Task AddInvestmentAsync(Investment investment) => await _investments.InsertOneAsync(investment);
    public async Task<IEnumerable<Investment>> GetInvestmentsByAccountAsync(Guid accountId) =>
        await _investments.Find(x => x.AccountId == accountId && x.LiquidatedAt == null).ToListAsync();
    public async Task UpdateInvestmentAsync(Investment investment) =>
        await _investments.ReplaceOneAsync(x => x.Id == investment.Id, investment);

    public async Task AddGoalAsync(SavingsGoal goal) => await _goals.InsertOneAsync(goal);
    public async Task<IEnumerable<SavingsGoal>> GetGoalsByAccountAsync(Guid accountId) =>
        await _goals.Find(x => x.AccountId == accountId).ToListAsync();
    public async Task<SavingsGoal> GetGoalByIdAsync(Guid goalId) =>
        await _goals.Find(x => x.Id == goalId).FirstOrDefaultAsync();
    public async Task UpdateGoalAsync(SavingsGoal goal) =>
        await _goals.ReplaceOneAsync(x => x.Id == goal.Id, goal);
}
