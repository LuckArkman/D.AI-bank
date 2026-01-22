using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Fintech.Records;

namespace Fintech.Web.Services;

public class BankingService
{
    private readonly HttpClient _httpClient;

    public BankingService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<BalanceResponse?> GetBalanceAsync(Guid accountId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"api/v1/accounts/{accountId}/balance");

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<BalanceResponse>();
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            return new BalanceResponse(accountId, 0, $"HTTP {(int)response.StatusCode}: {errorContent}");
        }
        catch (Exception ex)
        {
            return new BalanceResponse(accountId, 0, $"Erro de Conexão: {ex.Message}");
        }
    }

    public async Task<List<StatementItem>?> GetStatementAsync(Guid accountId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"api/v1/accounts/{accountId}/statement");
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<List<StatementItem>>();
            }
        }
        catch { }
        return new List<StatementItem>();
    }

    public async Task<bool> SendPixAsync(string key, decimal amount)
    {
        var response = await _httpClient.PostAsJsonAsync("api/v1/operations/transfer/pix", new { Key = key, Amount = amount });
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> TransferP2PAsync(Guid targetAccountId, decimal amount)
    {
        var response = await _httpClient.PostAsJsonAsync("api/v1/operations/transfer/p2p", new { TargetAccountId = targetAccountId, Amount = amount });
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DepositAsync(decimal amount)
    {
        var response = await _httpClient.PostAsJsonAsync("api/v1/operations/deposit", new { Amount = amount });
        return response.IsSuccessStatusCode;
    }
}

// Mapeamento explícito para bater com o Swagger da API
public record BalanceResponse(Guid AccountId, decimal AvailableBalance,
    string? Debug = null);

public record StatementItem(Guid Id, string Type, decimal Amount, DateTime Timestamp, Guid CorrelationId);
