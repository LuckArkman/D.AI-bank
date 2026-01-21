namespace Fintech.ValueObjects;

public record TimeZoneInfo
{
    public string Id { get; init; }
    public string DisplayName { get; init; }
    public TimeSpan BaseUtcOffset { get; init; }

    private TimeZoneInfo(string id, string displayName, TimeSpan baseUtcOffset)
    {
        Id = id;
        DisplayName = displayName;
        BaseUtcOffset = baseUtcOffset;
    }

    // Major time zones
    public static TimeZoneInfo BRT => new("E. South America Standard Time", "Brasília Time", TimeSpan.FromHours(-3));
    public static TimeZoneInfo EST => new("Eastern Standard Time", "Eastern Standard Time", TimeSpan.FromHours(-5));
    public static TimeZoneInfo PST => new("Pacific Standard Time", "Pacific Standard Time", TimeSpan.FromHours(-8));
    public static TimeZoneInfo GMT => new("GMT Standard Time", "Greenwich Mean Time", TimeSpan.Zero);
    public static TimeZoneInfo CET => new("Central European Standard Time", "Central European Time", TimeSpan.FromHours(1));
    public static TimeZoneInfo JST => new("Tokyo Standard Time", "Japan Standard Time", TimeSpan.FromHours(9));
    public static TimeZoneInfo AEST => new("AUS Eastern Standard Time", "Australian Eastern Standard Time", TimeSpan.FromHours(10));
    public static TimeZoneInfo CST_China => new("China Standard Time", "China Standard Time", TimeSpan.FromHours(8));

    public static TimeZoneInfo FromId(string id)
    {
        return id switch
        {
            "E. South America Standard Time" or "BRT" => BRT,
            "Eastern Standard Time" or "EST" => EST,
            "Pacific Standard Time" or "PST" => PST,
            "GMT Standard Time" or "GMT" => GMT,
            "Central European Standard Time" or "CET" => CET,
            "Tokyo Standard Time" or "JST" => JST,
            "AUS Eastern Standard Time" or "AEST" => AEST,
            "China Standard Time" or "CST" => CST_China,
            _ => throw new ArgumentException($"Time zone '{id}' is not supported")
        };
    }

    public DateTime ConvertFromUtc(DateTime utcDateTime)
    {
        return utcDateTime.Add(BaseUtcOffset);
    }

    public DateTime ConvertToUtc(DateTime localDateTime)
    {
        return localDateTime.Subtract(BaseUtcOffset);
    }
}
