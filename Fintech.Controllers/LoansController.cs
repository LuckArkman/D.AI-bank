using Fintech.Commands;
using Fintech.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fintech.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/v1/loans")]
public class LoansController : ControllerBase
{
    private readonly ILoanRepository _loanRepo;
    private readonly ICurrentUser _currentUser;

    public LoansController(ILoanRepository loanRepo, ICurrentUser currentUser)
    {
        _loanRepo = loanRepo;
        _currentUser = currentUser;
    }

    [HttpGet]
    public async Task<IActionResult> List()
    {
        var loans = await _loanRepo.GetByAccountIdAsync(_currentUser.AccountId);
        return Ok(loans);
    }

    [HttpPost("request")]
    public async Task<IActionResult> RequestLoan([FromBody] LoanRequestDTO request, [FromServices] RequestLoanHandler handler)
    {
        var loanId = await handler.Handle(_currentUser.AccountId, request.Amount, request.Installments);
        return Ok(new { LoanId = loanId });
    }
}

public record LoanRequestDTO(decimal Amount, int Installments);
