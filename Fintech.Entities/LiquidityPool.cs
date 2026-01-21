using Fintech.ValueObjects;

namespace Fintech.Entities;

public class LiquidityPool
{
    public Guid Id { get; private set; }
    public string Network { get; private set; } // SWIFT, SEPA
    public string CurrencyCode { get; private set; }
    public decimal TotalBalance { get; private set; }
    public decimal ReservedBalance { get; private set; }
    public DateTime LastUpdated { get; private set; }

    public LiquidityPool(string network, string currencyCode, decimal initialBalance = 0)
    {
        Id = Guid.NewGuid();
        Network = network;
        CurrencyCode = currencyCode;
        TotalBalance = initialBalance;
        LastUpdated = DateTime.UtcNow;
    }

    public void Deposit(decimal amount)
    {
        TotalBalance += amount;
        LastUpdated = DateTime.UtcNow;
    }

    public void Withdraw(decimal amount)
    {
        if (TotalBalance - ReservedBalance < amount)
            throw new Exception($"Insufficient liquidity in pool {Network}-{CurrencyCode}");

        TotalBalance -= amount;
        LastUpdated = DateTime.UtcNow;
    }

    public void Reserve(decimal amount)
    {
        if (TotalBalance - ReservedBalance < amount)
            throw new Exception($"Insufficient liquidity for reservation in pool {Network}-{CurrencyCode}");

        ReservedBalance += amount;
    }

    public void Release(decimal amount)
    {
        ReservedBalance -= amount;
    }
}
