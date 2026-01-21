using Fintech.Entities;

namespace Fintech.Interfaces;

public interface ILiquidityRepository
{
    Task<LiquidityPool?> GetByNetworkAndCurrencyAsync(string network, string currencyCode);
    Task UpdateAsync(LiquidityPool pool);
    Task AddAsync(LiquidityPool pool);
    Task<IEnumerable<LiquidityPool>> GetAllAsync();
}
