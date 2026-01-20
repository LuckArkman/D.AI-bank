using Fintech.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fintech.Controllers;

[ApiController]
[Route("api/v1/open-banking")]
[Authorize] // Exige autenticação
public class OpenBankingController : ControllerBase
{
    private readonly IOpenBankingService _openBankingService;

    public OpenBankingController(IOpenBankingService openBankingService)
    {
        _openBankingService = openBankingService;
    }

    [HttpGet("accounts")]
    public async Task<IActionResult> GetAccounts()
    {
        var accounts = await _openBankingService.GetAccountsAsync();
        return Ok(accounts);
    }

    [HttpGet("accounts/{accountId}/balance")]
    public async Task<IActionResult> GetBalance(Guid accountId)
    {
        var balance = await _openBankingService.GetBalanceAsync(accountId);
        if (balance == null) return NotFound();
        return Ok(balance);
    }

    [HttpGet("accounts/{accountId}/transactions")]
    public async Task<IActionResult> GetTransactions(Guid accountId)
    {
        var transactions = await _openBankingService.GetTransactionsAsync(accountId);
        return Ok(transactions);
    }
}
