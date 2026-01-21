using Fintech.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Fintech.Controllers;

[ApiController]
[Route("api/[controller]")]
public class KycController : ControllerBase
{
    private readonly IKycProvider _kycProvider;

    public KycController(IKycProvider kycProvider)
    {
        _kycProvider = kycProvider;
    }

    [HttpPost("verify")]
    public async Task<IActionResult> VerifyIdentity([FromBody] KycVerificationRequest request)
    {
        var result = await _kycProvider.VerifyIdentityAsync(request.UserId, request.DocumentImageUrl, request.SelfieImageUrl);

        if (result.Success)
        {
            return Ok(new { Message = "Identity verified successfully", Score = result.Score });
        }

        return BadRequest(new { Message = "Identity verification failed", Reason = result.FailureReason });
    }
}

public record KycVerificationRequest(Guid UserId, string DocumentImageUrl, string SelfieImageUrl);
