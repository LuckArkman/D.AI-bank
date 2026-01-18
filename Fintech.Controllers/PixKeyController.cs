using Fintech.Application.Commands;
using Fintech.Core.Interfaces;
using Fintech.Interfaces;
using Fintech.Records;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fintech.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/v1/pix/keys")]
public class PixKeyController : ControllerBase
{
    private readonly ICurrentUser _currentUser;
    private readonly IPixKeyRepository _pixRepo;

    public PixKeyController(ICurrentUser currentUser, IPixKeyRepository pixRepo)
    {
        _currentUser = currentUser;
        _pixRepo = pixRepo;
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreatePixKeyRequest request,
        [FromServices] RegisterPixKeyHandler handler)
    {
        await handler.Handle(_currentUser.AccountId, request.Key, request.Type);
        return Created("", new { request.Key });
    }

    [HttpGet]
    public async Task<IActionResult> List()
    {
        var keys = await _pixRepo.GetByAccountIdAsync(_currentUser.AccountId);
        
        var response = keys.Select(k => new PixKeyResponse(k.Key, k.Type, k.CreatedAt));
        return Ok(response);
    }
}