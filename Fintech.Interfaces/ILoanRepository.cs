using Fintech.Entities;

namespace Fintech.Interfaces;

public interface ILoanRepository
{
    Task AddAsync(Loan loan);
    Task<IEnumerable<Loan>> GetByAccountIdAsync(Guid accountId);
    Task<Loan> GetByIdAsync(Guid loanId);
    Task UpdateAsync(Loan loan);
}
