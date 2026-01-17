using Fintech.Commands;
using Fintech.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace Fintech.Controllers;

[ApiController]
[Route("api/accounts")]
public class AccountsController : ControllerBase
{
    private readonly DebitAccountHandler _debitHandler;

    public AccountsController(DebitAccountHandler debitHandler)
    {
        _debitHandler = debitHandler;
    }

    [HttpPost("{accountId}/debit")]
    public async Task<IActionResult> Debit(Guid accountId, [FromBody] DebitRequest request)
    {
        // 1. Extrair Idempotency-Key do Header (Padrão de mercado)
        if (!Request.Headers.TryGetValue("Idempotency-Key", out var idKeyString) || 
            !Guid.TryParse(idKeyString, out var commandId))
        {
            return BadRequest("Header 'Idempotency-Key' (GUID) é obrigatório.");
        }

        try
        {
            // O Handler cuida de tudo: Transação, Idempotência, Outbox
            await _debitHandler.HandleAsync(accountId, request.Amount, commandId);
            return Accepted(); // 202 - Processado (ou enfileirado)
        }
        catch (ConcurrencyException)
        {
            return Conflict("O recurso foi modificado. Tente novamente.");
        }
        catch (InvalidOperationException ex)
        {
            return UnprocessableEntity(ex.Message); // 422 - Saldo insuficiente
        }
    }
}

public record DebitRequest(decimal Amount);