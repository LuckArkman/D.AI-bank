using Fintech.Entities;
using Fintech.Interfaces;
using Fintech.Persistence;
using MongoDB.Driver;

namespace Fintech.Repositories;

public class LoanRepository : ILoanRepository
{
    private readonly IMongoCollection<Loan> _collection;
    private readonly ITenantProvider _tenantProvider;

    public LoanRepository(MongoContext context, ITenantProvider tenantProvider)
    {
        _collection = context.Database.GetCollection<Loan>("loans");
        _tenantProvider = tenantProvider;
    }

    public async Task AddAsync(Loan loan) => await _collection.InsertOneAsync(loan);

    public async Task<IEnumerable<Loan>> GetByAccountIdAsync(Guid accountId)
    {
        return await _collection.Find(x => x.AccountId == accountId && x.TenantId == _tenantProvider.TenantId).ToListAsync();
    }

    public async Task<Loan> GetByIdAsync(Guid loanId)
    {
        return await _collection.Find(x => x.Id == loanId && x.TenantId == _tenantProvider.TenantId).FirstOrDefaultAsync();
    }

    public async Task UpdateAsync(Loan loan)
    {
        await _collection.ReplaceOneAsync(x => x.Id == loan.Id && x.TenantId == _tenantProvider.TenantId, loan);
    }
}
