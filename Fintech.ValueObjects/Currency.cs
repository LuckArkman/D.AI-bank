namespace Fintech.ValueObjects;

public record Currency
{
    public string Code { get; init; }
    public string Symbol { get; init; }
    public string Name { get; init; }
    public int DecimalPlaces { get; init; }

    private Currency(string code, string symbol, string name, int decimalPlaces)
    {
        Code = code;
        Symbol = symbol;
        Name = name;
        DecimalPlaces = decimalPlaces;
    }

    // Major currencies
    public static Currency BRL => new("BRL", "R$", "Brazilian Real", 2);
    public static Currency USD => new("USD", "$", "US Dollar", 2);
    public static Currency EUR => new("EUR", "€", "Euro", 2);
    public static Currency GBP => new("GBP", "£", "British Pound", 2);
    public static Currency JPY => new("JPY", "¥", "Japanese Yen", 0);
    public static Currency CAD => new("CAD", "C$", "Canadian Dollar", 2);
    public static Currency AUD => new("AUD", "A$", "Australian Dollar", 2);
    public static Currency CHF => new("CHF", "Fr", "Swiss Franc", 2);
    public static Currency CNY => new("CNY", "¥", "Chinese Yuan", 2);
    public static Currency MXN => new("MXN", "$", "Mexican Peso", 2);
    public static Currency ARS => new("ARS", "$", "Argentine Peso", 2);

    // Cryptocurrencies
    public static Currency BTC => new("BTC", "₿", "Bitcoin", 8);
    public static Currency ETH => new("ETH", "Ξ", "Ethereum", 18);
    public static Currency USDT => new("USDT", "₮", "Tether", 6);

    public static Currency FromCode(string code)
    {
        return code.ToUpper() switch
        {
            "BRL" => BRL,
            "USD" => USD,
            "EUR" => EUR,
            "GBP" => GBP,
            "JPY" => JPY,
            "CAD" => CAD,
            "AUD" => AUD,
            "CHF" => CHF,
            "CNY" => CNY,
            "MXN" => MXN,
            "ARS" => ARS,
            "BTC" => BTC,
            "ETH" => ETH,
            "USDT" => USDT,
            _ => throw new ArgumentException($"Currency code '{code}' is not supported")
        };
    }

    public decimal Round(decimal amount)
    {
        return Math.Round(amount, DecimalPlaces);
    }
}
