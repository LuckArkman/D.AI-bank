using Fintech.ValueObjects;

namespace Fintech.Interfaces;

public interface ICurrencyExchangeService
{
    Task<decimal> GetExchangeRateAsync(Currency fromCurrency, Currency toCurrency);
    Task<Money> ConvertAsync(Money amount, Currency targetCurrency);
    Task<Dictionary<string, decimal>> GetExchangeRatesAsync(Currency baseCurrency);
}
