using Fintech.Interfaces;
using Fintech.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fintech.Controllers;

[Authorize(Roles = "Admin")]
[ApiController]
[Route("api/v1/admin/liquidity")]
public class LiquidityAdminController : ControllerBase
{
    private readonly ILiquidityRepository _liquidityRepo;
    private readonly ILiquidityService _liquidityService;

    public LiquidityAdminController(ILiquidityRepository liquidityRepo, ILiquidityService liquidityService)
    {
        _liquidityRepo = liquidityRepo;
        _liquidityService = liquidityService;
    }

    [HttpGet]
    public async Task<IActionResult> GetStatus()
    {
        var pools = await _liquidityRepo.GetAllAsync();
        return Ok(pools);
    }

    [HttpPost("seed")]
    public async Task<IActionResult> SeedLiquidity([FromBody] SeedLiquidityRequest request)
    {
        await _liquidityService.RegisterInflowAsync(request.Network, request.CurrencyCode, request.Amount);
        return Ok(new { Message = "Liquidity added successfully" });
    }

    [HttpPost("rebalance")]
    public async Task<IActionResult> Rebalance([FromBody] RebalanceRequest request)
    {
        await _liquidityService.RebalanceAsync(request.SourceNetwork, request.TargetNetwork, request.CurrencyCode, request.Amount);
        return Ok(new { Message = "Rebalance initiated via Web3 Bridge" });
    }
}

public record SeedLiquidityRequest(string Network, string CurrencyCode, decimal Amount);
public record RebalanceRequest(string SourceNetwork, string TargetNetwork, string CurrencyCode, decimal Amount);
