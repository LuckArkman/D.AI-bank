using Fintech.Commands;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Fintech.Interfaces;
using Fintech.Records;
using Fintech.Repositories;

namespace Fintech.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/v1/accounts")]
public class AccountController : ControllerBase
{
    private readonly ICurrentUser _currentUser;
    private readonly AccountRepository _accountRepo;

    public AccountController(ICurrentUser currentUser, AccountRepository accountRepo)
    {
        _currentUser = currentUser;
        _accountRepo = accountRepo;
    }

    [HttpGet("balance")]
    public async Task<IActionResult> GetBalance()
    {
        var account = await _accountRepo.GetByIdAsync(_currentUser.AccountId);
        
        // Retorna apenas o saldo BRL por enquanto
        var balance = account.Balances.ContainsKey("BRL") ? account.Balances["BRL"].Amount : 0;
        
        return Ok(new BalanceResponse(_currentUser.AccountId, balance));
    }

    [HttpGet("statement")]
    public async Task<IActionResult> GetStatement([FromServices] GetStatementHandler handler)
    {
        var result = await handler.Handle(_currentUser.AccountId);
        return Ok(result);
    }
}