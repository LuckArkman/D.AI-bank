using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Fintech.Commands;
using Fintech.Interfaces;
using Fintech.DTOs;
using Fintech.Entities;
using Fintech.Repositories;
using Fintech.Core.Interfaces;
using Fintech.ValueObjects;

namespace Fintech.Controllers;

[Authorize]
[ApiController]
[Route("api/v1/[controller]")]
public class TransferController : ControllerBase
{
    private readonly IAccountRepository _accountRepo;
    private readonly DebitAccountHandler _handler;
    private readonly ICurrentUser _currentUser;
    private readonly ITenantProvider _tenantProvider;

    public TransferController(IAccountRepository accountRepo, DebitAccountHandler handler, ICurrentUser currentUser, ITenantProvider tenantProvider)
    {
        _accountRepo = accountRepo;
        _handler = handler;
        _currentUser = currentUser;
        _tenantProvider = tenantProvider;
    }


    [HttpPost("debit")]
    public async Task<IActionResult> Debit([FromBody] DebitRequest request)
    {
        var correlationId = Guid.NewGuid();
        // O ID da conta vem do Token, nunca do body
        await _handler.Handle(_currentUser.AccountId, request.Amount, correlationId);
        return Accepted();
    }

#if DEBUG
    [HttpPost("setup")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> SetupAccount([FromBody] decimal initialBalance)
    {
        // Normalmente usaria um CreateAccountHandler separado
        // Aqui simulamos a criação manual inserindo direto no banco para agilizar o teste
        var id = Guid.NewGuid();
        var tenantId = _tenantProvider.TenantId ?? throw new Exception("TenantId não resolvido.");
        var acc = new Account(id, tenantId);

        // Hack para injetar saldo inicial sem criar método Credit agora
        acc.Balances["BRL"] = Money.BRL(initialBalance);

        // Inserção direta via Repo (assumindo injeção)
        await _accountRepo.AddAsync(acc);

        return Ok(new { AccountId = id, Message = "Conta criada para testes" });
    }
#endif
}