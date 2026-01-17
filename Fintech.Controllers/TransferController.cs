using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Fintech.Commands;
using Fintech.Interfaces;
using Fintech.DTOs;

namespace Fintech.Controllers;

[Authorize]
[ApiController]
[Route("api/v1/[controller]")]
public class TransferController : ControllerBase
{
    private readonly DebitAccountHandler _handler;
    private readonly ICurrentUser _currentUser;

    public TransferController(DebitAccountHandler handler, ICurrentUser currentUser)
    {
        _handler = handler;
        _currentUser = currentUser;
    }

    [HttpPost("debit")]
    public async Task<IActionResult> Debit([FromBody] DebitRequest request)
    {
        var correlationId = Guid.NewGuid();
        // O ID da conta vem do Token, nunca do body
        await _handler.Handle(_currentUser.AccountId, request.Amount, correlationId);
        return Accepted();
    }
}