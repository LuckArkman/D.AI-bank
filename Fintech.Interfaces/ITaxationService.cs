using Fintech.Entities;

namespace Fintech.Interfaces;

public interface ITaxationService
{
    Task<decimal> CalculateTotalTaxAsync(Account account, decimal amount, string operationType);
}
