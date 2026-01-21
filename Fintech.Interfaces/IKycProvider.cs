namespace Fintech.Interfaces;

public record KycResult(bool Success, string Score, string? FailureReason = null);

public interface IKycProvider
{
    string ProviderName { get; }
    Task<KycResult> VerifyIdentityAsync(Guid userId, string documentImageUrl, string selfieImageUrl);
}
