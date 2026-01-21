namespace Fintech.Interfaces;

public interface ILiquidityService
{
    Task EnsureLiquidityAsync(string network, string currencyCode, decimal amount);
    Task RegisterOutflowAsync(string network, string currencyCode, decimal amount);
    Task RegisterInflowAsync(string network, string currencyCode, decimal amount);
}
