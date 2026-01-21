using Fintech.Interfaces;
using Microsoft.Extensions.Logging;

namespace Fintech.Services;

public class JumioKycProvider : IKycProvider
{
    private readonly ILogger<JumioKycProvider> _logger;

    public JumioKycProvider(ILogger<JumioKycProvider> logger)
    {
        _logger = logger;
    }

    public string ProviderName => "Jumio AI Verification";

    public async Task<KycResult> VerifyIdentityAsync(Guid userId, string documentImageUrl, string selfieImageUrl)
    {
        _logger.LogInformation("Iniciando Verificação KYC via Jumio para Usuário {UserId}", userId);

        // Simulação de processamento de imagem por IA
        await Task.Delay(1500);

        // Lógica de simulação de score
        if (documentImageUrl.Contains("invalid") || selfieImageUrl.Contains("fail"))
        {
            return new KycResult(false, "0.0", "Biometric mismatch or document tampering detected.");
        }

        return new KycResult(true, "0.98");
    }
}
