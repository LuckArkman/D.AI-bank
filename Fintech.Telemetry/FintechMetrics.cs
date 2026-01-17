using System.Diagnostics.Metrics;

namespace Fintech.Telemetry;

public static class FintechMetrics
{
    public static readonly string MeterName = "Fintech.Core";
    private static readonly Meter Meter = new(MeterName);
    private static readonly Counter<decimal> TotalDebitCounter = Meter.CreateCounter<decimal>("fintech_money_debited_total");
    private static readonly Counter<long> TransactionSuccessCounter = Meter.CreateCounter<long>("fintech_transactions_success_total");
    private static readonly Counter<long> TransactionFailureCounter = Meter.CreateCounter<long>("fintech_transactions_failure_total");

    public static void RecordDebit(decimal amount) => TotalDebitCounter.Add(amount);
    public static void RecordSuccess() => TransactionSuccessCounter.Add(1);
    public static void RecordFailure() => TransactionFailureCounter.Add(1);
}