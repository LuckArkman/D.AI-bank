using Fintech.Interfaces;
using Fintech.Entities;
using Microsoft.Extensions.Logging;

namespace Fintech.Services;

public class LiquidityService : ILiquidityService
{
    private readonly ILiquidityRepository _liquidityRepo;
    private readonly ILogger<LiquidityService> _logger;

    public LiquidityService(ILiquidityRepository liquidityRepo, ILogger<LiquidityService> logger)
    {
        _liquidityRepo = liquidityRepo;
        _logger = logger;
    }

    public async Task EnsureLiquidityAsync(string network, string currencyCode, decimal amount)
    {
        var pool = await _liquidityRepo.GetByNetworkAndCurrencyAsync(network, currencyCode);

        if (pool == null)
        {
            _logger.LogWarning("Liquidity pool {Network}-{Currency} not found. Creating a temporary trial pool.", network, currencyCode);
            pool = new LiquidityPool(network, currencyCode, 1000000); // 1M trial balance
            await _liquidityRepo.AddAsync(pool);
        }

        if (pool.TotalBalance - pool.ReservedBalance < amount)
        {
            throw new Exception($"Critical Liquidity Breach: {network}-{currencyCode} has only {pool.TotalBalance - pool.ReservedBalance} available but {amount} is required.");
        }
    }

    public async Task RegisterOutflowAsync(string network, string currencyCode, decimal amount)
    {
        var pool = await _liquidityRepo.GetByNetworkAndCurrencyAsync(network, currencyCode);
        if (pool == null) throw new Exception("Pool missing");

        pool.Withdraw(amount);
        await _liquidityRepo.UpdateAsync(pool);
        _logger.LogInformation("Liquidity outflow registered: {Amount} {Currency} from {Network}", amount, currencyCode, network);
    }

    public async Task RegisterInflowAsync(string network, string currencyCode, decimal amount)
    {
        var pool = await _liquidityRepo.GetByNetworkAndCurrencyAsync(network, currencyCode);

        if (pool == null)
        {
            pool = new LiquidityPool(network, currencyCode, 0);
            await _liquidityRepo.AddAsync(pool);
        }

        pool.Deposit(amount);
        await _liquidityRepo.UpdateAsync(pool);
        _logger.LogInformation("Liquidity inflow registered: {Amount} {Currency} to {Network}", amount, currencyCode, network);
    }
}
