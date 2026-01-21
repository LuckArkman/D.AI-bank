using Fintech.Interfaces;
using Fintech.ValueObjects;

namespace Fintech.Services;

public class CurrencyExchangeService : ICurrencyExchangeService
{
    // In production, this would call an external API like OpenExchangeRates, CurrencyLayer, etc.
    // For now, we'll use hardcoded rates for demonstration
    private readonly Dictionary<string, Dictionary<string, decimal>> _exchangeRates = new()
    {
        ["USD"] = new Dictionary<string, decimal>
        {
            ["BRL"] = 4.95m,
            ["EUR"] = 0.92m,
            ["GBP"] = 0.79m,
            ["JPY"] = 149.50m,
            ["CAD"] = 1.35m,
            ["AUD"] = 1.52m,
            ["CHF"] = 0.88m,
            ["CNY"] = 7.24m,
            ["MXN"] = 17.15m,
            ["ARS"] = 350.00m
        },
        ["BRL"] = new Dictionary<string, decimal>
        {
            ["USD"] = 0.20m,
            ["EUR"] = 0.19m,
            ["GBP"] = 0.16m
        },
        ["EUR"] = new Dictionary<string, decimal>
        {
            ["USD"] = 1.09m,
            ["BRL"] = 5.39m,
            ["GBP"] = 0.86m
        }
    };

    public Task<decimal> GetExchangeRateAsync(Currency fromCurrency, Currency toCurrency)
    {
        if (fromCurrency.Code == toCurrency.Code)
            return Task.FromResult(1.0m);

        if (_exchangeRates.TryGetValue(fromCurrency.Code, out var rates))
        {
            if (rates.TryGetValue(toCurrency.Code, out var rate))
                return Task.FromResult(rate);
        }

        // If direct rate not found, try inverse
        if (_exchangeRates.TryGetValue(toCurrency.Code, out var inverseRates))
        {
            if (inverseRates.TryGetValue(fromCurrency.Code, out var inverseRate))
                return Task.FromResult(1.0m / inverseRate);
        }

        throw new InvalidOperationException($"Exchange rate not available for {fromCurrency.Code} to {toCurrency.Code}");
    }

    public async Task<Money> ConvertAsync(Money amount, Currency targetCurrency)
    {
        var rate = await GetExchangeRateAsync(amount.Currency, targetCurrency);
        return amount.ConvertTo(targetCurrency, rate);
    }

    public Task<Dictionary<string, decimal>> GetExchangeRatesAsync(Currency baseCurrency)
    {
        if (_exchangeRates.TryGetValue(baseCurrency.Code, out var rates))
            return Task.FromResult(rates);

        return Task.FromResult(new Dictionary<string, decimal>());
    }
}
