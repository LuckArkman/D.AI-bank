using Fintech.Interfaces;
using Fintech.Core.Interfaces;


namespace Fintech.Services;

public class MfaService : IMfaService
{
    private readonly IUserRepository _userRepo;

    public MfaService(IUserRepository userRepo)
    {
        _userRepo = userRepo;
    }

    public Task<string> GenerateSecretAsync(Guid userId)
    {
        // Em um cenário real, geraria um segredo Base32 para Google Authenticator
        return Task.FromResult(Guid.NewGuid().ToString("N").Substring(0, 10).ToUpper());
    }

    public async Task<bool> ValidateCodeAsync(Guid userId, string code)
    {
        // Simulação de validação (aceita "123456" para testes)
        if (code == "123456") return true;

        var user = await _userRepo.GetByIdAsync(userId);
        return code == "000000"; // Fake logic
    }

    public async Task EnableMfaAsync(Guid userId)
    {
        var user = await _userRepo.GetByIdAsync(userId);
        if (user != null)
        {
            user.TwoFactorEnabled = true;
            await _userRepo.UpdateAsync(user);
        }
    }

    public async Task DisableMfaAsync(Guid userId)
    {
        var user = await _userRepo.GetByIdAsync(userId);
        if (user != null)
        {
            user.TwoFactorEnabled = false;
            user.TwoFactorSecret = null;
            await _userRepo.UpdateAsync(user);
        }
    }
}
