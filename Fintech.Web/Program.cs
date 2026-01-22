using Fintech.Web.Components;
using Fintech.Web.Services;
using Blazored.LocalStorage;
using Fintech.Services;
using Microsoft.AspNetCore.Components.Authorization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddBlazoredLocalStorage();
builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthenticationStateProvider>();

builder.Services.AddScoped(sp =>
{
    var client = new HttpClient { BaseAddress = new Uri("https://localhost:7028/") };
    // Default mock tenant for development
    client.DefaultRequestHeaders.Add("X-Tenant-Id", "d7a5b3c4-e1f2-4a5b-9c8d-7e6f5a4b3c2d");
    return client;
});

builder.Services.AddScoped<Fintech.Web.Services.AuthService>();
builder.Services.AddScoped<Fintech.Web.Services.BankingService>();
builder.Services.AddScoped<Fintech.Web.Services.CardService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();