using Fintech.DTOs;
using Fintech.Entities;
using Fintech.Interfaces;
using Fintech.Regulatory;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fintech.Controllers;

[ApiController]
[Route("api/v1/tenet/admin")]
// [Authorize(Roles = "Admin")] // Uncomment in production
public class ComplianceAdminController : ControllerBase
{
    private readonly IRuleRepository _ruleRepo;
    private readonly ITenantOnboardingService _onboardingService;
    private readonly IComplianceReportingService _reportingService;

    public ComplianceAdminController(
        IRuleRepository ruleRepo,
        ITenantOnboardingService onboardingService,
        IComplianceReportingService reportingService)
    {
        _ruleRepo = ruleRepo;
        _onboardingService = onboardingService;
        _reportingService = reportingService;
    }

    // --- Rule Management ---

    [HttpGet("rules")]
    public async Task<IActionResult> GetRules()
    {
        var rules = await _ruleRepo.GetAllAsync();
        return Ok(rules);
    }

    [HttpPost("rules")]
    public async Task<IActionResult> CreateRule([FromBody] BusinessRule rule)
    {
        if (string.IsNullOrEmpty(rule.ConditionExpression))
            return BadRequest("ConditionExpression is required");

        await _ruleRepo.AddAsync(rule);
        return CreatedAtAction(nameof(GetRules), new { id = rule.Id }, rule);
    }

    [HttpPut("rules/{id}")]
    public async Task<IActionResult> UpdateRule(string id, [FromBody] BusinessRule rule)
    {
        if (id != rule.Id) return BadRequest();
        await _ruleRepo.UpdateAsync(rule);
        return NoContent();
    }

    [HttpDelete("rules/{id}")]
    public async Task<IActionResult> DeleteRule(string id)
    {
        await _ruleRepo.DeleteAsync(id);
        return NoContent();
    }

    // --- Tenant Onboarding ---

    [HttpPost("onboard")]
    public async Task<IActionResult> OnboardTenant([FromBody] TenantOnboardingRequest request)
    {
        try
        {
            var response = await _onboardingService.OnboardAsync(request);
            return Ok(response);
        }
        catch (Exception ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
    }

    // --- Compliance Dashboard / Reporting ---

    [HttpGet("reports")]
    public async Task<IActionResult> GetReports([FromQuery] Guid? tenantId)
    {
        // In this context, tenantId might come from query if Super Admin, or from Token if Tenant Admin
        // For simplicity, we use the query param or ICurrentUser (not injected here but typically available)

        if (tenantId == null) return BadRequest("TenantId is required");

        var reports = await _reportingService.GetReportsAsync(tenantId.Value);
        return Ok(reports);
    }

    [HttpPost("reports/generate")]
    public async Task<IActionResult> GenerateReport([FromBody] GenerateReportRequest request)
    {
        var report = await _reportingService.GenerateReportAsync(request.TenantId, request.ReportType, request.StartDate, request.EndDate);
        return Ok(report);
    }

    public record GenerateReportRequest(Guid TenantId, string ReportType, DateTime StartDate, DateTime EndDate);
}
