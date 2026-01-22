using Fintech.Commands;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Fintech.Interfaces;
using Fintech.Core.Interfaces;
using Fintech.Records;
using Fintech.Repositories;

namespace Fintech.Api.Controllers;

//[Authorize]
[ApiController]
[Route("api/v1/accounts")]
public class AccountController : ControllerBase
{
    private readonly ICurrentUser _currentUser;
    private readonly IAccountRepository _accountRepo;
    private readonly ITenantProvider _tenantProvider;

    public AccountController(ICurrentUser currentUser, IAccountRepository accountRepo, ITenantProvider tenantProvider)
    {
        _currentUser = currentUser;
        _accountRepo = accountRepo;
        _tenantProvider = tenantProvider;
    }

    [HttpGet("{accountId}/balance")]
    public async Task<IActionResult> GetBalance(Guid accountId)
    {
        Console.WriteLine($"{nameof(GetBalance)} >> Account: {accountId}");
        try
        {
            var account = await _accountRepo.GetByIdAsync(accountId);
            var balance = account.Balances.ContainsKey("BRL") ? account.Balances["BRL"].Amount : 0;
            return Ok(new BalanceResponse(accountId, balance));
        }
        catch (KeyNotFoundException)
        {
            var debugInfo = $"AccountId: {accountId}, TenantId: {_tenantProvider.TenantId}";
            return NotFound(new { Error = "Conta não encontrada.", Debug = debugInfo });
        }
    }

    [HttpGet("{accountId}/statement")]
    public async Task<IActionResult> GetStatement(Guid accountId, [FromServices] GetStatementHandler handler)
    {
        var result = await handler.Handle(accountId);
        return Ok(result);
    }
}