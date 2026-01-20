using Fintech.Entities;
using Fintech.Interfaces;
using Fintech.Persistence;
using MongoDB.Driver;

namespace Fintech.Repositories;

public class LoanRepository : ILoanRepository
{
    private readonly IMongoCollection<Loan> _collection;

    public LoanRepository(MongoContext context)
    {
        _collection = context.Database.GetCollection<Loan>("loans");
    }

    public async Task AddAsync(Loan loan) => await _collection.InsertOneAsync(loan);

    public async Task<IEnumerable<Loan>> GetByAccountIdAsync(Guid accountId)
    {
        return await _collection.Find(x => x.AccountId == accountId).ToListAsync();
    }

    public async Task<Loan> GetByIdAsync(Guid loanId)
    {
        return await _collection.Find(x => x.Id == loanId).FirstOrDefaultAsync();
    }

    public async Task UpdateAsync(Loan loan)
    {
        await _collection.ReplaceOneAsync(x => x.Id == loan.Id, loan);
    }
}
