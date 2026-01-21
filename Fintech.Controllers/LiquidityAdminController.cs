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
}

public record SeedLiquidityRequest(string Network, string CurrencyCode, decimal Amount);
