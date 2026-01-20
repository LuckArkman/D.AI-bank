using Fintech.Records;
using System.Net.Http;

namespace Fintech.Services;

public interface IPixGateway
{
    Task<PixGatewayResponse> SendPixAsync(string key, decimal amount);
}

public record PixGatewayResponse(bool Success, string? TransactionId, string? ErrorCode);

public class PixGateway : IPixGateway
{
    private static readonly Random _random = new();
    private readonly HttpClient _httpClient;

    public PixGateway(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<PixGatewayResponse> SendPixAsync(string key, decimal amount)
    {
        // Simula latência de rede externa (em um cenário real, usaríamos o _httpClient)
        await Task.Delay(_random.Next(500, 2000));

        // Simula falhas aleatórias (10%)
        if (_random.Next(1, 100) <= 10)
        {
            return new PixGatewayResponse(false, null, "EXTERNAL_GATEWAY_ERROR");
        }

        return new PixGatewayResponse(true, Guid.NewGuid().ToString("N"), null);
    }
}
