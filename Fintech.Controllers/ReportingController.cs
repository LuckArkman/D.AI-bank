using Fintech.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace Fintech.Controllers;

[Authorize(Roles = "Admin")]
[ApiController]
[Route("api/v1/admin/reports")]
public class ReportingController : ControllerBase
{
    private readonly IComplianceReportingService _reportingService;
    private readonly ITenantProvider _tenantProvider;

    public ReportingController(IComplianceReportingService reportingService, ITenantProvider tenantProvider)
    {
        _reportingService = reportingService;
        _tenantProvider = tenantProvider;
    }

    [HttpGet("generate")]
    public async Task<IActionResult> DownloadReport([FromQuery] string type = "GENERAL")
    {
        var tenantId = _tenantProvider.TenantId ?? throw new Exception("Tenant missing");
        var report = await _reportingService.GenerateReportAsync(tenantId, type, DateTime.UtcNow.AddMonths(-1), DateTime.UtcNow);

        var csv = new StringBuilder();
        csv.AppendLine("Metric,Value");
        foreach (var item in report.Data)
        {
            csv.AppendLine($"{item.Key},{item.Value}");
        }

        var bytes = Encoding.UTF8.GetBytes(csv.ToString());
        return File(bytes, "text/csv", $"ComplianceReport_{type}_{DateTime.UtcNow:yyyyMMdd}.csv");
    }
}
