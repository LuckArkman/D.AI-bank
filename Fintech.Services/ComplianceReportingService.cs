using Fintech.Interfaces;
using Fintech.Core.Interfaces;

namespace Fintech.Services;

public class ComplianceReportingService : IComplianceReportingService
{
    private readonly ILedgerRepository _ledgerRepo;

    public ComplianceReportingService(ILedgerRepository ledgerRepo)
    {
        _ledgerRepo = ledgerRepo;
    }

    public async Task<ComplianceReport> GenerateReportAsync(Guid tenantId, string reportType, DateTime startDate, DateTime endDate)
    {
        // 1. Fetch data from Ledger
        var events = await _ledgerRepo.GetByTenantAndDateRangeAsync(tenantId, startDate, endDate);
        var ledgerList = events.ToList();

        // 2. Aggregate Data
        var totalTransactions = ledgerList.Count;
        var totalVolume = ledgerList
            .Where(e => e.Type.Contains("SENT") || e.Type.Contains("DEBIT"))
            .Sum(e => e.Amount);

        var flaggedTransactions = ledgerList.Count(e => e.Amount > 10000); // Simple flagging logic

        var report = new ComplianceReport
        {
            TenantId = tenantId,
            ReportType = reportType,
            GeneratedAt = DateTime.UtcNow,
            StartDate = startDate,
            EndDate = endDate,
            Status = "COMPLETED",
            DownloadUrl = $"https://reports.tenet.finance/{tenantId}/{reportType}/{Guid.NewGuid()}.pdf",
            Data = new Dictionary<string, object>
            {
                ["TotalTransactions"] = totalTransactions,
                ["TotalVolume"] = totalVolume,
                ["FlaggedTransactions"] = flaggedTransactions,
                ["AccountCount"] = ledgerList.Select(e => e.AccountId).Distinct().Count()
            }
        };

        // In a real scenario, we would save this report record to a database too
        return report;
    }

    public Task<List<ComplianceReport>> GetReportsAsync(Guid tenantId)
    {
        // Dummy list for now
        return Task.FromResult(new List<ComplianceReport>());
    }
}
