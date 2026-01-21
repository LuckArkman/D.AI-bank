namespace Fintech.ValueObjects;

public record Money
{
    public decimal Amount { get; init; }
    public Currency Currency { get; init; }

    private Money(decimal amount, Currency currency)
    {
        if (currency == null) throw new ArgumentException("Currency is required");
        Amount = currency.Round(amount);
        Currency = currency;
    }

    public static Money Create(decimal amount, string currencyCode)
        => new(amount, Currency.FromCode(currencyCode));

    public static Money BRL(decimal amount) => new(amount, Currency.BRL);
    public static Money USD(decimal amount) => new(amount, Currency.USD);
    public static Money EUR(decimal amount) => new(amount, Currency.EUR);
    public static Money GBP(decimal amount) => new(amount, Currency.GBP);
    public static Money JPY(decimal amount) => new(amount, Currency.JPY);

    public static Money operator +(Money a, Money b)
    {
        if (a.Currency.Code != b.Currency.Code)
            throw new InvalidOperationException($"Cannot add {a.Currency.Code} and {b.Currency.Code}. Use currency conversion first.");
        return new Money(a.Amount + b.Amount, a.Currency);
    }

    public static Money operator -(Money a, Money b)
    {
        if (a.Currency.Code != b.Currency.Code)
            throw new InvalidOperationException($"Cannot subtract {a.Currency.Code} and {b.Currency.Code}. Use currency conversion first.");
        return new Money(a.Amount - b.Amount, a.Currency);
    }

    public static bool operator >(Money a, Money b)
    {
        if (a.Currency.Code != b.Currency.Code)
            throw new InvalidOperationException($"Cannot compare {a.Currency.Code} and {b.Currency.Code}. Use currency conversion first.");
        return a.Amount > b.Amount;
    }

    public static bool operator <(Money a, Money b)
    {
        if (a.Currency.Code != b.Currency.Code)
            throw new InvalidOperationException($"Cannot compare {a.Currency.Code} and {b.Currency.Code}. Use currency conversion first.");
        return a.Amount < b.Amount;
    }

    public Money ConvertTo(Currency targetCurrency, decimal exchangeRate)
    {
        if (Currency.Code == targetCurrency.Code) return this;
        var convertedAmount = Amount * exchangeRate;
        return new Money(convertedAmount, targetCurrency);
    }

    public override string ToString() => $"{Currency.Symbol}{Amount:N2}";
}
