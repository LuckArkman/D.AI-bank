using Fintech.Interfaces;
using Microsoft.Extensions.Logging;

namespace Fintech.Services;

public class FraudDetectionService : IFraudDetectionService
{
    private readonly ILogger<FraudDetectionService> _logger;

    public FraudDetectionService(ILogger<FraudDetectionService> logger)
    {
        _logger = logger;
    }

    public async Task<bool> IsTransactionFraudulentAsync(Guid accountId, decimal amount, string? metadata)
    {
        _logger.LogInformation("Analisando fraude para conta {AccountId}, valor {Amount}", accountId, amount);

        // Simulando lógica de detecção de fraude
        // Em um cenário real, aqui usaríamos um modelo de ML ou regras complexas

        if (amount > 100000) // Transações muito altas sem histórico
        {
            _logger.LogWarning("Transação suspeita detectada para conta {AccountId}: Valor muito alto", accountId);
            return true;
        }

        await Task.Delay(200); // Simulando processamento ML
        return false;
    }

    public async Task<bool> IsFraudulentAsync(Guid accountId, decimal amount)
    {
        // Para simulação de crédito, negamos se o valor for exorbitante para um score básico
        if (amount > 500000) return true;

        await Task.Delay(100);
        return false;
    }
}

