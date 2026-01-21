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
using Fintech.Regulatory;
using Fintech.Regulatory.Packs;
using Fintech.Regulatory.Rules;
using Fintech.Services.ProductModules;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;
using Polly;
using Polly.Extensions.Http;


var builder = WebApplication.CreateBuilder(args);

// 1. Configs de Infraestrutura
BsonSerializer.RegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));
MongoClassMaps.Register();
builder.Services.AddSingleton<IMongoClient>(sp => new MongoClient(builder.Configuration.GetConnectionString("Mongo") ?? "mongodb://mplopes:3702959@localhost:27017/"));
builder.Services.AddScoped<MongoContext>();
builder.Services.AddScoped<ITransactionManager>(sp => sp.GetRequiredService<MongoContext>());
builder.Services.AddStackExchangeRedisCache(o => o.Configuration = builder.Configuration.GetConnectionString("Redis") ?? "localhost:6379");

// 1.1 Health Checks
builder.Services.AddHealthChecks()
    .AddMongoDb(
        mongodbConnectionString: builder.Configuration.GetConnectionString("Mongo") ?? "mongodb://mplopes:3702959@localhost:27017/",
        name: "mongodb",
        timeout: TimeSpan.FromSeconds(3))
    .AddRedis(
        redisConnectionString: builder.Configuration.GetConnectionString("Redis") ?? "localhost:6379",
        name: "redis",
        timeout: TimeSpan.FromSeconds(3))
    .AddRabbitMQ(
        rabbitConnectionString: "amqp://guest:guest@localhost:5672", // Default for dev
        name: "rabbitmq",
        timeout: TimeSpan.FromSeconds(3));

// 1.2 CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("Default", policy =>
    {
        policy.WithOrigins("http://localhost:3000") // Exemplo de frontend
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// 1.3 Rate Limiting
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("strict", opt =>
    {
        opt.Window = TimeSpan.FromSeconds(10);
        opt.PermitLimit = 5;
        opt.QueueLimit = 0;
    });

    options.AddSlidingWindowLimiter("api", opt =>
    {
        opt.Window = TimeSpan.FromMinutes(1);
        opt.PermitLimit = 100;
        opt.QueueLimit = 20;
        opt.SegmentsPerWindow = 4;
    });
});


// 2. Repositories
builder.Services.AddScoped<IAccountRepository, AccountRepository>();
builder.Services.AddScoped<ISagaRepository, SagaRepository>();
builder.Services.AddScoped<ILedgerRepository, LedgerRepository>();
builder.Services.AddScoped<IOutboxRepository, OutboxRepository>();
builder.Services.AddScoped<IPixKeyRepository, PixKeyRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ICardRepository, CardRepository>();
builder.Services.AddScoped<ILoanRepository, LoanRepository>();
builder.Services.AddScoped<IInvestmentRepository, InvestmentRepository>();
builder.Services.AddScoped<ITenantRepository, TenantRepository>();
builder.Services.AddScoped<AccountRepository>();
builder.Services.AddScoped<SagaRepository>();
builder.Services.AddScoped<LedgerRepository>();
builder.Services.AddScoped<PixKeyRepository>();
builder.Services.AddScoped<UserRepository>();
builder.Services.AddScoped<TenantRepository>();
builder.Services.AddScoped<IRuleRepository, RuleRepository>();
builder.Services.AddScoped<RuleRepository>();


// 3. Services
builder.Services.AddScoped<IPixOrchestrator, PixOrchestrator>();
builder.Services.AddHttpClient<IPixGateway, PixGateway>()

    .AddPolicyHandler(HttpPolicyExtensions
        .HandleTransientHttpError()
        .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.NotFound)
        .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))))
    .AddPolicyHandler(HttpPolicyExtensions
        .HandleTransientHttpError()
        .CircuitBreakerAsync(5, TimeSpan.FromSeconds(30)));

builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<IdempotencyService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IFraudDetectionService, FraudDetectionService>();
builder.Services.AddScoped<IOpenBankingService, OpenBankingService>();
builder.Services.AddScoped<IMfaService, MfaService>();
builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddScoped<ICurrencyExchangeService, CurrencyExchangeService>();
builder.Services.AddScoped<IComplianceReportingService, ComplianceReportingService>();
builder.Services.AddScoped<ITenantOnboardingService, TenantOnboardingService>();
builder.Services.AddSingleton<IMessageBus, RabbitMqClient>();

// Tenet Regulatory
builder.Services.AddSingleton<IBusinessRulesEngine, BusinessRulesEngine>();
builder.Services.AddSingleton<IRegulatoryRegistry>(sp =>
{
    var registry = new RegulatoryRegistry();
    var rulesEngine = sp.GetRequiredService<IBusinessRulesEngine>();
    registry.RegisterPack(new BrazilRegulatoryPack(rulesEngine));
    registry.RegisterPack(new USRegulatoryPack(rulesEngine));
    registry.RegisterPack(new EURegulatoryPack(rulesEngine));
    registry.RegisterPack(new UKRegulatoryPack(rulesEngine));
    return registry;
});
builder.Services.AddScoped<IRegulatoryService, RegulatoryService>();

// Tenet Product Modules
builder.Services.AddScoped<IProductModule, CryptoWalletModule>();
builder.Services.AddScoped<IProductModule, CardsModule>();
builder.Services.AddScoped<IProductModule, LoansModule>();






// 4. Handlers e Commands
builder.Services.AddScoped<RegisterPixKeyHandler>();
builder.Services.AddScoped<DepositHandler>();
builder.Services.AddScoped<TransferFundsHandler>();
builder.Services.AddScoped<GetStatementHandler>();
builder.Services.AddScoped<SendPixHandler>();
builder.Services.AddScoped<DebitAccountHandler>();
builder.Services.AddScoped<IssueCardHandler>();
builder.Services.AddScoped<RequestLoanHandler>();
builder.Services.AddScoped<CardActionHandler>();
builder.Services.AddScoped<InvestmentHandler>();


// --- CORREÇÃO PRINCIPAL AQUI ---
// Registra a Interface vinculada à Implementação
builder.Services.AddScoped<ICreateAccountHandler, CreateAccountHandler>();
// Se precisar resolver a classe concreta também em outros lugares (ex: CLI), pode manter a linha abaixo, mas a de cima é a que corrige o erro do AuthService:
builder.Services.AddScoped<CreateAccountHandler>();

// 5. Auth & User Context
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUser, HttpContextCurrentUser>();
builder.Services.AddScoped<ITenantProvider, TenantProvider>();

// Configuração JWT
var jwtSecret = builder.Configuration["Jwt:Secret"] ?? "Segredo_Super_Secreto_Para_Dev_Local_123!";
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(x =>
    {
        x.RequireHttpsMetadata = !builder.Environment.IsDevelopment();

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

    // Inicializa Índices do MongoDB em desenvolvimento
    using var scope = app.Services.CreateScope();
    var mongoClient = scope.ServiceProvider.GetRequiredService<IMongoClient>();
    var db = mongoClient.GetDatabase("FintechDB");
    await MongoDbIndexes.CreateIndexes(db);
}

app.UseHttpsRedirection();
app.UseHsts();
app.UseCors("Default");
app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.UseMiddleware<IdempotencyMiddleware>();
app.MapControllers();

app.Run();
