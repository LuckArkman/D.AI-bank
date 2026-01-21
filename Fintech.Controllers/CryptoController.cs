using Fintech.Interfaces;
using Fintech.Services;
using Microsoft.AspNetCore.Mvc;

namespace Fintech.Controllers;

[ApiController]
[Route("api/v1/crypto")]
public class CryptoController : ControllerBase
{
    private readonly ICryptoService _cryptoService;

    public CryptoController(ICryptoService cryptoService)
    {
        _cryptoService = cryptoService;
    }

    [HttpGet("wallet/{accountId}")]
    public async Task<IActionResult> GetWallet(Guid accountId)
    {
        var wallet = await _cryptoService.GetWalletAsync(accountId);
        return Ok(wallet);
    }

    [HttpPost("buy")]
    public async Task<IActionResult> Buy([FromBody] CryptoTradeRequest request)
    {
        try
        {
            await _cryptoService.BuyCryptoAsync(request.AccountId, request.Symbol, request.Amount);
            return Ok(new { Message = "Purchase successful" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
    }

    [HttpPost("sell")]
    public async Task<IActionResult> Sell([FromBody] CryptoTradeRequest request)
    {
        try
        {
            await _cryptoService.SellCryptoAsync(request.AccountId, request.Symbol, request.Amount);
            return Ok(new { Message = "Sale successful" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
    }

    public record CryptoTradeRequest(Guid AccountId, string Symbol, decimal Amount);
}
