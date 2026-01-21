using Fintech.Application.Commands;
using Fintech.Commands;
using Fintech.Interfaces;
using Fintech.Records;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fintech.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/v1/operations")]
public class OperationsController : ControllerBase
{
    private readonly ICurrentUser _currentUser;

    public OperationsController(ICurrentUser currentUser)
    {
        _currentUser = currentUser;
    }

    [HttpPost("transfer/p2p")]
    public async Task<IActionResult> TransferP2P(
        [FromBody] P2PTransferRequest request,
        [FromServices] TransferFundsHandler handler)
    {
        var correlationId = Guid.NewGuid();

        await handler.Handle(
            fromAccountId: _currentUser.AccountId,
            toAccountId: request.TargetAccountId,
            amount: request.Amount,
            currencyCode: request.CurrencyCode ?? "BRL"
        );

        return Accepted(new { Message = "Transferência realizada com sucesso.", OperationId = correlationId });
    }

    [HttpPost("transfer/international")]
    public async Task<IActionResult> IntlTransfer(
        [FromBody] InternationalTransferRequest request,
        [FromServices] InternationalTransferHandler handler)
    {
        var reference = await handler.Handle(
            fromAccountId: _currentUser.AccountId,
            amount: request.Amount,
            currencyCode: request.CurrencyCode,
            network: request.Network,
            destinationBank: request.DestinationBank,
            destinationAccount: request.DestinationAccount
        );

        return Accepted(new
        {
            Message = "International transfer initiated via " + request.Network,
            TransactionReference = reference
        });
    }

    [HttpPost("transfer/pix")]
    public async Task<IActionResult> SendPix(
        [FromBody] PixSendRequest request,
        [FromServices] SendPixHandler handler)
    {
        var sagaId = await handler.Handle(_currentUser.AccountId, request.Key, request.Amount);

        return Accepted(new
        {
            Message = "Pix em processamento.",
            SagaId = sagaId,
            StatusUrl = $"/api/v1/operations/pix/{sagaId}"
        });
    }

    [HttpPost("deposit")]
    public async Task<IActionResult> Deposit(
        [FromBody] DepositRequest request,
        [FromServices] DepositHandler handler)
    {
        // Em um cenário real, aqui geraríamos um Boleto e o crédito só ocorreria via Webhook.
        // Para este MVP, simulamos o crédito imediato (Sandbox).
        await handler.Handle(_currentUser.AccountId, request.Amount);

        return Ok(new { Message = "Depósito recebido com sucesso." });
    }
}