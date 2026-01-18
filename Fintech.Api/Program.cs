using Fintech.Interfaces;
using Fintech.Middlewares;
using Fintech.Persistence;
using Fintech.Api.Services;
using MongoDB.Driver;
using System.Text;
using Fintech.Application.Commands;
using Fintech.Commands;
using Fintech.Core.Interfaces;
using Fintech.Repositories;
using Fintech.Messaging;
using Fintech.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

var builder = WebApplication.CreateBuilder(args);

// 1. Configs de Infraestrutura
BsonSerializer.RegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));
MongoClassMaps.Register();
builder.Services.AddSingleton<IMongoClient>(sp => new MongoClient(builder.Configuration.GetConnectionString("Mongo") ?? "mongodb://mplopes:3702959@localhost:27017/"));
builder.Services.AddScoped<MongoContext>();
builder.Services.AddScoped<ITransactionManager>(sp => sp.GetRequiredService<MongoContext>());
builder.Services.AddStackExchangeRedisCache(o => o.Configuration = builder.Configuration.GetConnectionString("Redis") ?? "localhost:6379");

// 2. Repositories
builder.Services.AddScoped<AccountRepository>();
builder.Services.AddScoped<SagaRepository>();
builder.Services.AddScoped<LedgerRepository>();
builder.Services.AddScoped<IOutboxRepository, OutboxRepository>();
builder.Services.AddScoped<IPixOrchestrator, PixOrchestrator>();
builder.Services.AddScoped<IPixKeyRepository, PixKeyRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IAccountRepository, AccountRepository>();
builder.Services.AddScoped<ILedgerRepository, LedgerRepository>();

// 3. Services
builder.Services.AddScoped<PixOrchestrator>();
builder.Services.AddScoped<IPixGateway, PixGateway>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<IdempotencyService>();
builder.Services.AddSingleton<IMessageBus, RabbitMqClient>();

// 4. Handlers e Commands
builder.Services.AddScoped<RegisterPixKeyHandler>();
builder.Services.AddScoped<DepositHandler>();
builder.Services.AddScoped<TransferFundsHandler>();
builder.Services.AddScoped<GetStatementHandler>();
builder.Services.AddScoped<SendPixHandler>();
builder.Services.AddScoped<DebitAccountHandler>();

// --- CORREÇÃO PRINCIPAL AQUI ---
// Registra a Interface vinculada à Implementação
builder.Services.AddScoped<ICreateAccountHandler, CreateAccountHandler>(); 
// Se precisar resolver a classe concreta também em outros lugares (ex: CLI), pode manter a linha abaixo, mas a de cima é a que corrige o erro do AuthService:
builder.Services.AddScoped<CreateAccountHandler>(); 

// 5. Auth & User Context
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUser, HttpContextCurrentUser>();

// Configuração JWT
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

// 6. Controllers e Swagger
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