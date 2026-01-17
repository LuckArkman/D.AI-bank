namespace Fintech.ValueObjects;

public record Money
{
    public decimal Amount { get; init; }
    public string Currency { get; init; }

    private Money(decimal amount, string currency)
    {
        if (string.IsNullOrWhiteSpace(currency)) throw new ArgumentException("Moeda obrigatória");
        Amount = amount;
        Currency = currency.ToUpper();
    }

    public static Money BRL(decimal amount) => new(amount, "BRL");
    public static Money USD(decimal amount) => new(amount, "USD");

    public static Money operator +(Money a, Money b)
    {
        if (a.Currency != b.Currency) throw new InvalidOperationException("Moedas diferentes");
        return new Money(a.Amount + b.Amount, a.Currency);
    }

    public static Money operator -(Money a, Money b)
    {
        if (a.Currency != b.Currency) throw new InvalidOperationException("Moedas diferentes");
        return new Money(a.Amount - b.Amount, a.Currency);
    }
    
    public static bool operator >(Money a, Money b) => a.Amount > b.Amount;
    public static bool operator <(Money a, Money b) => a.Amount < b.Amount;
}