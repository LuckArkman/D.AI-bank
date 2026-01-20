using Fintech.Commands;
using Fintech.Enums;
using Fintech.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fintech.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/v1/investments")]
public class InvestmentController : ControllerBase
{
    private readonly IInvestmentRepository _investmentRepo;
    private readonly ICurrentUser _currentUser;
    private readonly InvestmentHandler _handler;

    public InvestmentController(IInvestmentRepository investmentRepo, ICurrentUser currentUser, InvestmentHandler handler)
    {
        _investmentRepo = investmentRepo;
        _currentUser = currentUser;
        _handler = handler;
    }

    [HttpGet]
    public async Task<IActionResult> GetInvestments()
    {
        var investments = await _investmentRepo.GetInvestmentsByAccountAsync(_currentUser.AccountId);
        return Ok(investments);
    }

    [HttpPost]
    public async Task<IActionResult> Invest([FromBody] InvestRequest request)
    {
        await _handler.InvestAsync(_currentUser.AccountId, request.Name, request.Type, request.Amount);
        return Ok(new { Message = "Investimento realizado com sucesso." });
    }

    [HttpGet("goals")]
    public async Task<IActionResult> GetGoals()
    {
        var goals = await _investmentRepo.GetGoalsByAccountAsync(_currentUser.AccountId);
        return Ok(goals);
    }

    [HttpPost("goals")]
    public async Task<IActionResult> CreateGoal([FromBody] CreateGoalRequest request)
    {
        await _handler.CreateGoalAsync(_currentUser.AccountId, request.Name, request.TargetAmount, request.Color);
        return Ok(new { Message = "Caixinha criada com sucesso." });
    }

    [HttpPost("goals/{id}/deposit")]
    public async Task<IActionResult> DepositToGoal(Guid id, [FromBody] DepositGoalRequest request)
    {
        await _handler.AddToGoalAsync(_currentUser.AccountId, id, request.Amount);
        return Ok(new { Message = "Saldo adicionado à caixinha." });
    }
}

public record InvestRequest(string Name, InvestmentType Type, decimal Amount);
public record CreateGoalRequest(string Name, decimal TargetAmount, string Color);
public record DepositGoalRequest(decimal Amount);
