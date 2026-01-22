using System.Net.Http.Json;
using Fintech.Enums;
using Fintech.Records;

namespace Fintech.Web.Services;

public class CardService
{
    private readonly HttpClient _httpClient;

    public CardService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<CardDto>?> ListCardsAsync()
    {
        return await _httpClient.GetFromJsonAsync<List<CardDto>>("api/v1/cards");
    }

    public async Task<bool> IssueCardAsync(IssueCardRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync("api/v1/cards/issue", request);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> ToggleBlockAsync(Guid cardId)
    {
        var response = await _httpClient.PostAsync($"api/v1/cards/{cardId}/toggle-block", null);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> UpdateLimitAsync(Guid cardId, decimal newLimit)
    {
        var response = await _httpClient.PatchAsJsonAsync($"api/v1/cards/{cardId}/limit", new { NewLimit = newLimit });
        return response.IsSuccessStatusCode;
    }
}

public record CardDto(Guid Id, string CardNumber, string CardHolderName, string ExpirationDate, string Cvv, string Brand, CardType Type, bool IsVirtual, decimal CreditLimit, bool IsBlocked);
public record IssueCardRequest(string Brand, CardType Type, bool IsVirtual, decimal CreditLimit = 0);
