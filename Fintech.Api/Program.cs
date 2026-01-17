using Fintech.Interfaces;
using Fintech.Middlewares;
using Fintech.Persistence;
using Fintech.Api.Services;
using MongoDB.Driver;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// 1. Configs de Infraestrutura
MongoClassMaps.Register();
builder.Services.AddSingleton<IMongoClient>(sp => new MongoClient(builder.Configuration.GetConnectionString("Mongo") ?? "mongodb://localhost:27017"));
builder.Services.AddScoped<MongoContext>();
builder.Services.AddScoped<ITransactionManager>(sp => sp.GetRequiredService<MongoContext>());
builder.Services.AddStackExchangeRedisCache(o => o.Configuration = builder.Configuration.GetConnectionString("Redis") ?? "localhost:6379");

// 2. Repositories
builder.Services.AddScoped<Fintech.Repositories.AccountRepository>();
builder.Services.AddScoped<Fintech.Repositories.SagaRepository>();
builder.Services.AddScoped<Fintech.Repositories.LedgerRepository>();
builder.Services.AddScoped<IOutboxRepository, Fintech.Repositories.OutboxRepository>();

// 3. Services (Correção Aqui)
builder.Services.AddScoped<Fintech.Services.PixOrchestrator>();
builder.Services.AddScoped<Fintech.Services.IPixGateway, Fintech.Services.PixGateway>();
builder.Services.AddScoped<Fintech.Services.AuthService>();
builder.Services.AddSingleton<IMessageBus, Fintech.Messaging.RabbitMqClient>();

// CORREÇÃO: Registrando o IdempotencyService que causou o erro
builder.Services.AddScoped<Fintech.Services.IdempotencyService>();

// 4. Handlers (Correção Aqui)
builder.Services.AddScoped<Fintech.Commands.SendPixHandler>();
builder.Services.AddScoped<Fintech.Commands.DebitAccountHandler>();
builder.Services.AddScoped<Fintech.Commands.TransferFundsHandler>();

// CORREÇÃO: Registrando CreateAccountHandler (dependência do AuthService)
builder.Services.AddScoped<Fintech.Commands.CreateAccountHandler>();

// 5. Auth & User Context (Necessário para os Controllers)
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUser, HttpContextCurrentUser>();

// Configuração JWT Básica (Para evitar erro 500 no startup se Auth for usada)
var jwtSecret = builder.Configuration["Jwt:Secret"] ?? "Segredo_Super_Secreto_Para_Dev_Local_123!";
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(x =>
    {
        x.RequireHttpsMetadata = false;
        x.SaveToken = true;
        x.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtSecret)),
            ValidateIssuer = false,
            ValidateAudience = false
        };
    });
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<IdempotencyMiddleware>();
app.MapControllers();

app.Run();