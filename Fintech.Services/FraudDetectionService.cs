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
        _logger.LogInformation("Predicting fraud score for account {AccountId}, value {Amount}", accountId, amount);

        // AI Feature Engineering Simulation
        double riskScore = 0.0;

        // Rule 1: High Amount relative to "typical" behavior
        if (amount > 50000) riskScore += 0.4;

        // Rule 2: Geolocation anomalies (simulated by metadata)
        if (metadata != null && metadata.Contains("unknown_location")) riskScore += 0.3;

        // Rule 3: Velocity Check (simulated)
        // In a real database, we would check how many transactions in the last hour
        riskScore += 0.1; // Baseline velocity risk

        _logger.LogInformation("Calculated Fraud Risk Score: {Score}", riskScore);

        await Task.Delay(300); // Simulate model inference time

        if (riskScore >= 0.7)
        {
            _logger.LogWarning("AI Prediction: FRAUD DETECTED (Score: {Score}) for account {AccountId}", riskScore, accountId);
            return true;
        }

        return false;
    }

    public async Task<bool> IsFraudulentAsync(Guid accountId, decimal amount)
    {
        // Simple onboarding fraud check
        if (amount > 1000000) return true;
        return false;
    }
}

