namespace Fintech.Interfaces;

public interface IComplianceReportingService
{
    Task<ComplianceReport> GenerateReportAsync(Guid tenantId, string reportType, DateTime startDate, DateTime endDate);
    Task<List<ComplianceReport>> GetReportsAsync(Guid tenantId);
}

public record ComplianceReport
{
    public string Id { get; init; } = Guid.NewGuid().ToString();
    public Guid TenantId { get; init; }
    public string ReportType { get; init; }
    public DateTime GeneratedAt { get; init; }
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
    public string Status { get; init; }
    public string DownloadUrl { get; init; }
    public Dictionary<string, object> Data { get; init; }
}
