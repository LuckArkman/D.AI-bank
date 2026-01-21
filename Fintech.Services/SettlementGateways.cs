using Fintech.Interfaces;
using Fintech.ValueObjects;
using Microsoft.Extensions.Logging;

namespace Fintech.Services;

public class SwiftGateway : ISettlementGateway
{
    private readonly ILogger<SwiftGateway> _logger;

    public SwiftGateway(ILogger<SwiftGateway> logger)
    {
        _logger = logger;
    }

    public string Network => "SWIFT";

    public async Task<SettlementResponse> ProcessSettlementAsync(Guid transactionId, Money amount, string destinationBank, string destinationAccount)
    {
        _logger.LogInformation("Escalando para rede SWIFT: {Amount} {Currency} para {Bank}", amount.Amount, amount.Currency.Code, destinationBank);

        // Simulação de latência de rede interbancária
        await Task.Delay(2000);

        return new SettlementResponse(true, $"SWIFT-{Guid.NewGuid():N}");
    }
}

public class SepaGateway : ISettlementGateway
{
    private readonly ILogger<SepaGateway> _logger;

    public SepaGateway(ILogger<SepaGateway> logger)
    {
        _logger = logger;
    }

    public string Network => "SEPA";

    public async Task<SettlementResponse> ProcessSettlementAsync(Guid transactionId, Money amount, string destinationBank, string destinationAccount)
    {
        _logger.LogInformation("Processando Transação SEPA: {Amount} EUR", amount.Amount);

        if (amount.Currency.Code != "EUR")
            return new SettlementResponse(false, "", "SEPA network only support EUR");

        await Task.Delay(800);
        return new SettlementResponse(true, $"SEPA-{Guid.NewGuid():N}");
    }
}
