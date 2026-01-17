using System.Diagnostics.Metrics;

namespace Fintech.Telemetry;

public static class FintechMetrics
{
    public static readonly string MeterName = "Fintech.Core";
    private static readonly Meter Meter = new(MeterName);
    private static readonly Counter<decimal> TotalDebitCounter = Meter.CreateCounter<decimal>("fintech_money_debited_total");

    public static void RecordDebit(decimal amount) => TotalDebitCounter.Add(amount);
}