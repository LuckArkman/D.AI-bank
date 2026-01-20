using Fintech.Commands;
using Fintech.Enums;
using Fintech.Interfaces;
using Fintech.Records;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fintech.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/v1/cards")]
public class CardsController : ControllerBase
{
    private readonly ICardRepository _cardRepo;
    private readonly ICurrentUser _currentUser;

    public CardsController(ICardRepository cardRepo, ICurrentUser currentUser)
    {
        _cardRepo = cardRepo;
        _currentUser = currentUser;
    }

    [HttpGet]
    public async Task<IActionResult> List()
    {
        var cards = await _cardRepo.GetByAccountIdAsync(_currentUser.AccountId);
        return Ok(cards);
    }

    [HttpPost("issue")]
    public async Task<IActionResult> Issue([FromBody] IssueCardRequest request, [FromServices] IssueCardHandler handler)
    {
        var cardId = await handler.Handle(_currentUser.AccountId, request.Brand, request.Type, request.IsVirtual, request.CreditLimit);
        return Ok(new { CardId = cardId });
    }
}

public record IssueCardRequest(string Brand, CardType Type, bool IsVirtual, decimal CreditLimit = 0);
