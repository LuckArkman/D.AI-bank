namespace Fintech.Interfaces;

public interface IMfaService
{
    Task<string> GenerateSecretAsync(Guid userId);
    Task<bool> ValidateCodeAsync(Guid userId, string code);
    Task EnableMfaAsync(Guid userId);
    Task DisableMfaAsync(Guid userId);
}
