using Fintech.Entities;
using Fintech.Interfaces;
using Fintech.Persistence;
using MongoDB.Driver;

namespace Fintech.Repositories;

public class InvestmentRepository : IInvestmentRepository
{
    private readonly IMongoCollection<Investment> _investments;
    private readonly IMongoCollection<SavingsGoal> _goals;
    private readonly ITenantProvider _tenantProvider;

    public InvestmentRepository(MongoContext context, ITenantProvider tenantProvider)
    {
        _investments = context.Database.GetCollection<Investment>("investments");
        _goals = context.Database.GetCollection<SavingsGoal>("goals");
        _tenantProvider = tenantProvider;
    }

    public async Task AddInvestmentAsync(Investment investment) => await _investments.InsertOneAsync(investment);
    public async Task<IEnumerable<Investment>> GetInvestmentsByAccountAsync(Guid accountId) =>
        await _investments.Find(x => x.AccountId == accountId && x.TenantId == _tenantProvider.TenantId && x.LiquidatedAt == null).ToListAsync();
    public async Task UpdateInvestmentAsync(Investment investment) =>
        await _investments.ReplaceOneAsync(x => x.Id == investment.Id && x.TenantId == _tenantProvider.TenantId, investment);

    public async Task AddGoalAsync(SavingsGoal goal) => await _goals.InsertOneAsync(goal);
    public async Task<IEnumerable<SavingsGoal>> GetGoalsByAccountAsync(Guid accountId) =>
        await _goals.Find(x => x.AccountId == accountId && x.TenantId == _tenantProvider.TenantId).ToListAsync();
    public async Task<SavingsGoal> GetGoalByIdAsync(Guid goalId) =>
        await _goals.Find(x => x.Id == goalId && x.TenantId == _tenantProvider.TenantId).FirstOrDefaultAsync();
    public async Task UpdateGoalAsync(SavingsGoal goal) =>
        await _goals.ReplaceOneAsync(x => x.Id == goal.Id && x.TenantId == _tenantProvider.TenantId, goal);
}
