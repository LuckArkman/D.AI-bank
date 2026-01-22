using System.Net.Http.Json;
using Blazored.LocalStorage;
using Fintech.Records;
using Microsoft.AspNetCore.Components.Authorization;

namespace Fintech.Web.Services;

public class AuthService
{
    private readonly HttpClient _httpClient;
    private readonly ILocalStorageService _localStorage;
    private readonly AuthenticationStateProvider _authStateProvider;

    public AuthService(HttpClient httpClient, ILocalStorageService localStorage, AuthenticationStateProvider authStateProvider)
    {
        _httpClient = httpClient;
        _localStorage = localStorage;
        _authStateProvider = authStateProvider;
    }

    public async Task<string?> RegisterAsync(RegisterRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync("api/v1/auth/register", request);

        if (response.IsSuccessStatusCode)
        {
            return null;
        }

        try
        {
            if (response.Content.Headers.ContentType?.MediaType == "application/json")
            {
                var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
                return error?.Error ?? "Erro desconhecido ao criar conta.";
            }
            else
            {
                var errorText = await response.Content.ReadAsStringAsync();
                return !string.IsNullOrEmpty(errorText) ? errorText : "Falha na comunicação com o servidor.";
            }
        }
        catch
        {
            return "Erro ao processar resposta do servidor.";
        }
    }

    public async Task<string?> LoginAsync(LoginRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync("api/v1/auth/login", request);

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<AuthResponse>();
            if (result != null)
            {
                await _localStorage.SetItemAsync("token", result.Token);
                ((CustomAuthenticationStateProvider)_authStateProvider).NotifyUserAuthentication(result.Token);
                return null;
            }
        }

        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        return error?.Error ?? "Falha na autenticação.";
    }

    public async Task LogoutAsync()
    {
        await _localStorage.RemoveItemAsync("token");
        ((CustomAuthenticationStateProvider)_authStateProvider).NotifyUserLogout();
    }
}

public record AuthResponse(string Token);
public record ErrorResponse(string Error);
