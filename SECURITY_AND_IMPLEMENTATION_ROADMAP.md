# 🔒 Roadmap de Segurança e Correções - D.AI Bank

**Data da Análise:** 2026-01-20  
**Analista:** Antigravity AI  
**Projeto:** D.AI Bank - Core Banking Engine

---

## 📋 Sumário Executivo

Este documento apresenta uma análise abrangente de **falhas de implementação** e **riscos de segurança** identificados no projeto D.AI Bank. A análise cobriu todos os diretórios e scripts do projeto, incluindo código-fonte, configurações, infraestrutura e testes.

### Classificação de Severidade
- 🔴 **CRÍTICO**: Requer ação imediata - Risco de segurança grave ou falha que pode causar perda de dados
- 🟠 **ALTO**: Requer atenção urgente - Vulnerabilidade significativa ou bug importante
- 🟡 **MÉDIO**: Deve ser corrigido - Problema que afeta qualidade ou segurança moderadamente
- 🟢 **BAIXO**: Melhoria recomendada - Boas práticas e otimizações

---

## 🔴 RISCOS CRÍTICOS DE SEGURANÇA

### 1. Credenciais Hardcoded em Múltiplos Arquivos
**Severidade:** 🔴 CRÍTICO  
**Arquivos Afetados:**
- `main.tf` (linhas 21, 47)
- `Fintech.Api/appsettings.json` (linhas 10, 13)
- `Fintech.Api/Program.cs` (linha 24, 66)

**Problema:**
```terraform
# main.tf
master_password = "SecurePass123!" # Idealmente via Secret Manager
password = "SecurePass123!"
```

```json
// appsettings.json
"Jwt": {
  "Secret": "Segredo_Super_Secreto_Para_Dev_Local_123!"
},
"ConnectionStrings": {
  "Mongo": "mongodb://mplopes:3702959@localhost:27017/"
}
```

**Impacto:**
- Exposição de credenciais de banco de dados em repositório Git
- Chave JWT hardcoded permite falsificação de tokens
- Credenciais do MongoDB expostas (usuário: mplopes, senha: 3702959)
- Senhas do DocumentDB e RabbitMQ expostas no Terraform

**Solução Recomendada:**
1. **Imediato:**
   - Rotacionar TODAS as credenciais expostas
   - Remover credenciais do histórico Git (git filter-branch ou BFG Repo-Cleaner)
   
2. **Implementação:**
   - Usar Azure Key Vault, AWS Secrets Manager ou HashiCorp Vault
   - Implementar variáveis de ambiente para desenvolvimento local
   - Configurar User Secrets para desenvolvimento (.NET)
   - Usar Terraform variables com backend remoto seguro

```csharp
// Program.cs - Correção
var jwtSecret = builder.Configuration["Jwt:Secret"] 
    ?? Environment.GetEnvironmentVariable("JWT_SECRET")
    ?? throw new InvalidOperationException("JWT Secret não configurado");

var mongoConnection = builder.Configuration.GetConnectionString("Mongo")
    ?? Environment.GetEnvironmentVariable("MONGO_CONNECTION")
    ?? throw new InvalidOperationException("MongoDB connection não configurada");
```

---

### 2. Endpoint de Setup Sem Autenticação
**Severidade:** 🔴 CRÍTICO  
**Arquivo:** `Fintech.Controllers/TransferController.cs` (linhas 36-52)

**Problema:**
```csharp
[HttpPost("setup")]
[AllowAnonymous] // Apenas para teste local!
public async Task<IActionResult> SetupAccount([FromBody] decimal initialBalance)
{
    var id = Guid.NewGuid();
    var acc = new Account(id);
    acc.Balances["BRL"] = Money.BRL(initialBalance); 
    await _accountRepo.AddAsync(acc);
    return Ok(new { AccountId = id, Message = "Conta criada para testes" });
}
```

**Impacto:**
- Qualquer pessoa pode criar contas com saldo arbitrário
- Bypass completo do sistema de autenticação
- Manipulação direta de saldos sem auditoria
- Injeção de AccountRepository não inicializada (NullReferenceException)

**Solução Recomendada:**
1. **Remover completamente em produção** ou proteger com:
   - Autenticação de admin
   - Feature flag para ambiente de desenvolvimento
   - Rate limiting agressivo
   
2. **Alternativa segura:**
```csharp
[HttpPost("setup")]
[Authorize(Roles = "Admin")]
#if DEBUG
public async Task<IActionResult> SetupAccount([FromBody] decimal initialBalance)
{
    if (!_environment.IsDevelopment())
        return Forbid();
    
    // Implementação com logging de auditoria
}
#endif
```

---

### 3. Falta de Validação de Input em Endpoints Críticos
**Severidade:** 🔴 CRÍTICO  
**Arquivos:** Múltiplos controllers

**Problema:**
- Nenhuma validação de valores negativos em transferências
- Falta de limite máximo de transação
- Sem validação de formato de chave Pix
- Sem sanitização de inputs

**Exemplos:**
```csharp
// TransferController.cs - Permite valores negativos!
public async Task<IActionResult> Debit([FromBody] DebitRequest request)
{
    await _handler.Handle(_currentUser.AccountId, request.Amount, correlationId);
    // Sem validação se Amount > 0
}

// RegisterPixKeyHandler.cs - Validação fraca
if (!validTypes.Contains(type.ToUpper()))
    throw new DomainException("Tipo de chave Pix inválido.");
// Não valida formato da chave (CPF, email, etc)
```

**Solução Recomendada:**
```csharp
// DTOs com validação
public record DebitRequest
{
    [Required]
    [Range(0.01, 1000000, ErrorMessage = "Valor deve estar entre R$ 0,01 e R$ 1.000.000")]
    public decimal Amount { get; init; }
}

// Handler com validação adicional
public async Task Handle(Guid accountId, decimal amount, Guid correlationId)
{
    if (amount <= 0)
        throw new DomainException("Valor deve ser positivo");
    
    if (amount > 1000000)
        throw new DomainException("Valor excede limite de transação");
    
    // Continua...
}
```

---

### 4. Ausência de Rate Limiting
**Severidade:** 🔴 CRÍTICO  
**Arquivo:** `Fintech.Api/Program.cs`

**Problema:**
- Nenhum middleware de rate limiting configurado
- Vulnerável a ataques de força bruta em login
- Vulnerável a DoS em endpoints de transação
- Sem proteção contra enumeração de contas

**Solução Recomendada:**
```csharp
// Program.cs
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("auth", opt =>
    {
        opt.Window = TimeSpan.FromMinutes(1);
        opt.PermitLimit = 5;
    });
    
    options.AddSlidingWindowLimiter("transactions", opt =>
    {
        opt.Window = TimeSpan.FromMinutes(1);
        opt.PermitLimit = 100;
        opt.SegmentsPerWindow = 4;
    });
});

// Controller
[EnableRateLimiting("auth")]
[HttpPost("login")]
public async Task<IActionResult> Login([FromBody] LoginRequest request)
```

---

### 5. Falta de HTTPS Enforcement
**Severidade:** 🔴 CRÍTICO  
**Arquivo:** `Fintech.Api/Program.cs` (linha 70)

**Problema:**
```csharp
x.RequireHttpsMetadata = false; // PERIGOSO!
```

**Impacto:**
- Tokens JWT podem ser interceptados em texto claro
- Credenciais de login expostas em trânsito
- Vulnerável a ataques Man-in-the-Middle

**Solução Recomendada:**
```csharp
// Program.cs
app.UseHttpsRedirection();
app.UseHsts(); // HTTP Strict Transport Security

// JWT Configuration
x.RequireHttpsMetadata = !app.Environment.IsDevelopment();
```

---

## 🟠 RISCOS ALTOS DE SEGURANÇA

### 6. Falta de Proteção CSRF
**Severidade:** 🟠 ALTO  
**Arquivo:** `Fintech.Api/Program.cs`

**Problema:**
- Nenhuma proteção contra Cross-Site Request Forgery
- APIs REST sem validação de origem

**Solução Recomendada:**
```csharp
builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-CSRF-TOKEN";
});

// CORS configurado adequadamente
builder.Services.AddCors(options =>
{
    options.AddPolicy("Production", builder =>
    {
        builder.WithOrigins("https://daibank.com")
               .AllowCredentials()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});
```

---

### 7. Logs Podem Expor Informações Sensíveis
**Severidade:** 🟠 ALTO  
**Arquivos:** Múltiplos

**Problema:**
```csharp
// OutboxWorker.cs
_logger.LogDebug("Mensagem outbox {Id} processada.", msg.Id);
// Pode logar payloads com PII
```

**Solução Recomendada:**
- Implementar log sanitization
- Usar structured logging com mascaramento
- Configurar níveis de log adequados por ambiente

```csharp
public class SensitiveDataFilter : ILogger
{
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, 
        Exception exception, Func<TState, Exception, string> formatter)
    {
        var message = formatter(state, exception);
        message = Regex.Replace(message, @"\b\d{11}\b", "***CPF***"); // Mascara CPF
        message = Regex.Replace(message, @"\b[\w\.-]+@[\w\.-]+\.\w+\b", "***EMAIL***");
        // Log sanitizado
    }
}
```

---

### 8. Falta de Auditoria Completa
**Severidade:** 🟠 ALTO  
**Arquivo:** `Fintech.Entities/LedgerEvent.cs`

**Problema:**
- Ledger não registra quem executou a ação
- Falta de IP e device fingerprint
- Metadata opcional pode ser ignorada

**Solução Recomendada:**
```csharp
public class LedgerEvent
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid AccountId { get; set; }
    public Guid UserId { get; set; } // ADICIONAR
    public string Type { get; set; }
    public decimal Amount { get; set; }
    public decimal BalanceAfter { get; set; }
    public Guid CorrelationId { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    
    // Auditoria obrigatória
    public required AuditMetadata Audit { get; set; }
}

public record AuditMetadata(
    string IpAddress,
    string UserAgent,
    string DeviceFingerprint,
    Guid SessionId
);
```

---

### 9. Transações MongoDB Não Garantidas em Todos os Fluxos
**Severidade:** 🟠 ALTO  
**Arquivos:** 
- `Fintech.Commands/CreateAccountHandler.cs` (linhas 26, 43)
- `Fintech.Services/AuthService.cs` (linha 35)

**Problema:**
```csharp
// CreateAccountHandler.cs
//using var uow = await _txManager.BeginTransactionAsync(); // COMENTADO!
try
{
    var accountId = Guid.NewGuid();
    var account = new Account(accountId);
    await _accountRepo.AddAsync(account);
    // ...
    //await uow.CommitAsync(); // COMENTADO!
}
```

**Impacto:**
- Criação de conta e ledger podem ficar inconsistentes
- Registro de usuário sem conta bancária
- Violação de atomicidade ACID

**Solução Recomendada:**
```csharp
public async Task<Guid> Handle(decimal initialBalance)
{
    using var uow = await _txManager.BeginTransactionAsync();
    try
    {
        var accountId = Guid.NewGuid();
        var account = new Account(accountId);
        await _accountRepo.AddAsync(account);
        
        var ledger = new LedgerEvent(accountId, "ACCOUNT_CREATED", initialBalance, Guid.NewGuid());
        await _ledgerRepo.AddAsync(ledger);
        
        await uow.CommitAsync();
        return accountId;
    }
    catch
    {
        await uow.AbortAsync();
        throw;
    }
}
```

---

### 10. MongoContext Não Limpa Sessão Após Commit/Abort
**Severidade:** 🟠 ALTO  
**Arquivo:** `Fintech.Persistence/MongoContext.cs`

**Problema:**
```csharp
public async Task CommitAsync(CancellationToken ct) 
{ 
    await _session.CommitTransactionAsync(ct); 
    _session.Dispose(); 
    // _currentSession não é setado para null!
}
```

**Impacto:**
- Reutilização acidental de sessão encerrada
- Memory leaks em cenários de alta carga
- Exceções de sessão inválida

**Solução Recomendada:**
```csharp
private class MongoUnitOfWork : IUnitOfWork
{
    private readonly IClientSessionHandle _session;
    private readonly MongoContext _context;
    
    public MongoUnitOfWork(IClientSessionHandle session, MongoContext context)
    {
        _session = session;
        _context = context;
    }
    
    public async Task CommitAsync(CancellationToken ct = default)
    {
        await _session.CommitTransactionAsync(ct);
        _session.Dispose();
        _context._currentSession = null; // LIMPAR REFERÊNCIA
    }
    
    public async Task AbortAsync(CancellationToken ct = default)
    {
        await _session.AbortTransactionAsync(ct);
        _session.Dispose();
        _context._currentSession = null; // LIMPAR REFERÊNCIA
    }
    
    public void Dispose()
    {
        _session?.Dispose();
        _context._currentSession = null;
    }
}
```

---

## 🟡 RISCOS MÉDIOS E FALHAS DE IMPLEMENTAÇÃO

### 11. Idempotência Não Funciona Corretamente para Erros
**Severidade:** 🟡 MÉDIO  
**Arquivo:** `Fintech.Middlewares/IdempotencyMiddleware.cs` (linha 48)

**Problema:**
```csharp
if (context.Response.StatusCode >= 200 && context.Response.StatusCode < 300)
{
    await _cache.SetStringAsync(cacheKey, responseText, ...);
}
// Erros 4xx e 5xx não são cacheados, permitindo retry de operações que falharam
```

**Impacto:**
- Cliente pode retentar operação que falhou por saldo insuficiente
- Possível duplicação de transações em cenários de timeout

**Solução Recomendada:**
```csharp
// Cachear também erros de negócio (4xx)
if (context.Response.StatusCode >= 200 && context.Response.StatusCode < 500)
{
    var ttl = context.Response.StatusCode < 300 
        ? TimeSpan.FromHours(24) 
        : TimeSpan.FromMinutes(5);
    
    await _cache.SetStringAsync(cacheKey, responseText, 
        new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = ttl });
}
```

---

### 12. PixOrchestrator Usa Chave Pix Fake
**Severidade:** 🟡 MÉDIO  
**Arquivo:** `Fintech.Services/PixOrchestrator.cs` (linha 71)

**Problema:**
```csharp
private async Task HandleBalanceLocked(PixSaga saga)
{
    var response = await _pixGateway.SendPixAsync("fake-pix-key", saga.Amount);
    // Chave Pix hardcoded!
}
```

**Impacto:**
- Saga não usa a chave Pix real do destinatário
- Implementação incompleta do fluxo de Pix

**Solução Recomendada:**
```csharp
// PixSaga.cs - Adicionar propriedade
public string DestinationPixKey { get; private set; }

public PixSaga(Guid accountId, decimal amount, string destinationPixKey)
{
    // ...
    DestinationPixKey = destinationPixKey;
}

// PixOrchestrator.cs
private async Task HandleBalanceLocked(PixSaga saga)
{
    var response = await _pixGateway.SendPixAsync(saga.DestinationPixKey, saga.Amount);
    // ...
}
```

---

### 13. Falta de Timeout em Operações Assíncronas
**Severidade:** 🟡 MÉDIO  
**Arquivos:** Múltiplos handlers

**Problema:**
- Nenhuma operação async tem timeout configurado
- Possível deadlock em falhas de rede
- Worker pode ficar travado indefinidamente

**Solução Recomendada:**
```csharp
// Configurar timeouts globais
builder.Services.AddHttpClient<IPixGateway, PixGateway>()
    .ConfigureHttpClient(client =>
    {
        client.Timeout = TimeSpan.FromSeconds(30);
    });

// Usar CancellationToken em operações críticas
public async Task ProcessPixSaga(Guid sagaId, CancellationToken ct = default)
{
    using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
    cts.CancelAfter(TimeSpan.FromSeconds(60));
    
    var saga = await _sagaRepo.GetByIdAsync(sagaId, cts.Token);
    // ...
}
```

---

### 14. OutboxWorker Sem Controle de Concorrência
**Severidade:** 🟡 MÉDIO  
**Arquivo:** `Fintech.Worker/OutboxWorker.cs`

**Problema:**
```csharp
var messages = await outboxRepo.GetPendingAsync(20);
foreach (var msg in messages)
{
    await bus.PublishAsync(msg.Topic, msg.PayloadJson);
    await outboxRepo.MarkAsProcessedAsync(msg.Id);
}
// Se múltiplas instâncias do worker rodarem, podem processar a mesma mensagem
```

**Impacto:**
- Mensagens duplicadas no RabbitMQ
- Violação de exactly-once delivery

**Solução Recomendada:**
```csharp
// OutboxMessage.cs - Adicionar lock
public Guid? LockedBy { get; set; }
public DateTime? LockedAt { get; set; }

// OutboxRepository.cs
public async Task<List<OutboxMessage>> GetPendingAsync(int limit, Guid workerId)
{
    var filter = Builders<OutboxMessage>.Filter.And(
        Builders<OutboxMessage>.Filter.Eq(x => x.ProcessedAt, null),
        Builders<OutboxMessage>.Filter.Or(
            Builders<OutboxMessage>.Filter.Eq(x => x.LockedBy, null),
            Builders<OutboxMessage>.Filter.Lt(x => x.LockedAt, DateTime.UtcNow.AddMinutes(-5))
        )
    );
    
    var update = Builders<OutboxMessage>.Update
        .Set(x => x.LockedBy, workerId)
        .Set(x => x.LockedAt, DateTime.UtcNow);
    
    // Usar FindOneAndUpdate para lock atômico
}
```

---

### 15. Falta de Validação de Moeda em Operações
**Severidade:** 🟡 MÉDIO  
**Arquivo:** `Fintech.ValueObjects/Money.cs`

**Problema:**
```csharp
public static bool operator >(Money a, Money b) => a.Amount > b.Amount;
public static bool operator <(Money a, Money b) => a.Amount < b.Amount;
// Não valida se as moedas são iguais!
```

**Impacto:**
- Comparação de BRL com USD sem conversão
- Lógica de saldo insuficiente pode falhar

**Solução Recomendada:**
```csharp
public static bool operator >(Money a, Money b)
{
    if (a.Currency != b.Currency)
        throw new InvalidOperationException($"Não é possível comparar {a.Currency} com {b.Currency}");
    return a.Amount > b.Amount;
}

public static bool operator <(Money a, Money b)
{
    if (a.Currency != b.Currency)
        throw new InvalidOperationException($"Não é possível comparar {a.Currency} com {b.Currency}");
    return a.Amount < b.Amount;
}
```

---

### 16. Falta de Índices no MongoDB
**Severidade:** 🟡 MÉDIO  
**Arquivo:** Nenhum arquivo de migração/índices encontrado

**Problema:**
- Nenhuma configuração de índices MongoDB
- Queries podem ser lentas em produção
- Falta de índice único em chaves Pix

**Solução Recomendada:**
```csharp
// Criar MongoDbIndexes.cs
public static class MongoDbIndexes
{
    public static async Task CreateIndexes(IMongoDatabase db)
    {
        // Accounts
        var accountsCollection = db.GetCollection<Account>("accounts");
        await accountsCollection.Indexes.CreateOneAsync(
            new CreateIndexModel<Account>(
                Builders<Account>.IndexKeys.Ascending(x => x.Id),
                new CreateIndexOptions { Unique = true }
            )
        );
        
        // PixKeys - Índice único
        var pixKeysCollection = db.GetCollection<PixKey>("pixkeys");
        await pixKeysCollection.Indexes.CreateOneAsync(
            new CreateIndexModel<PixKey>(
                Builders<PixKey>.IndexKeys.Ascending(x => x.Key),
                new CreateIndexOptions { Unique = true }
            )
        );
        
        // Outbox - Índice composto para queries eficientes
        var outboxCollection = db.GetCollection<OutboxMessage>("outbox");
        await outboxCollection.Indexes.CreateOneAsync(
            new CreateIndexModel<OutboxMessage>(
                Builders<OutboxMessage>.IndexKeys
                    .Ascending(x => x.ProcessedAt)
                    .Ascending(x => x.CreatedAt)
            )
        );
        
        // Ledger - Índice para queries por conta e período
        var ledgerCollection = db.GetCollection<LedgerEvent>("ledger");
        await ledgerCollection.Indexes.CreateManyAsync(new[]
        {
            new CreateIndexModel<LedgerEvent>(
                Builders<LedgerEvent>.IndexKeys.Ascending(x => x.AccountId)
            ),
            new CreateIndexModel<LedgerEvent>(
                Builders<LedgerEvent>.IndexKeys.Descending(x => x.Timestamp)
            ),
            new CreateIndexModel<LedgerEvent>(
                Builders<LedgerEvent>.IndexKeys.Ascending(x => x.CorrelationId)
            )
        });
    }
}

// Program.cs
var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var mongoClient = scope.ServiceProvider.GetRequiredService<IMongoClient>();
    var db = mongoClient.GetDatabase("FintechDB");
    await MongoDbIndexes.CreateIndexes(db);
}
```

---

### 17. Falta de Health Checks
**Severidade:** 🟡 MÉDIO  
**Arquivo:** `Fintech.Api/Program.cs`

**Problema:**
- Nenhum health check configurado
- Impossível monitorar saúde da aplicação
- Kubernetes/Docker não pode verificar readiness

**Solução Recomendada:**
```csharp
// Program.cs
builder.Services.AddHealthChecks()
    .AddMongoDb(
        mongodbConnectionString: builder.Configuration.GetConnectionString("Mongo"),
        name: "mongodb",
        timeout: TimeSpan.FromSeconds(3))
    .AddRedis(
        redisConnectionString: builder.Configuration.GetConnectionString("Redis"),
        name: "redis",
        timeout: TimeSpan.FromSeconds(3))
    .AddRabbitMQ(
        rabbitConnectionString: builder.Configuration["RabbitMQ:ConnectionString"],
        name: "rabbitmq",
        timeout: TimeSpan.FromSeconds(3));

// Endpoint
app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});
```

---

### 18. Docker Compose Incompleto
**Severidade:** 🟡 MÉDIO  
**Arquivo:** `compose.yaml`

**Problema:**
```yaml
services:
  fintech.api:
    image: fintech.api
    build:
      context: .
      dockerfile: Fintech.Api/Dockerfile
  # Falta MongoDB, Redis, RabbitMQ!
```

**Impacto:**
- Impossível rodar stack completa com docker-compose
- Desenvolvedor precisa instalar dependências manualmente

**Solução Recomendada:**
```yaml
version: '3.8'

services:
  mongodb:
    image: mongo:7.0
    container_name: fintech-mongodb
    ports:
      - "27017:27017"
    environment:
      MONGO_INITDB_ROOT_USERNAME: ${MONGO_USER:-admin}
      MONGO_INITDB_ROOT_PASSWORD: ${MONGO_PASSWORD:-changeme}
    volumes:
      - mongodb_data:/data/db
    networks:
      - fintech-network

  redis:
    image: redis:7-alpine
    container_name: fintech-redis
    ports:
      - "6379:6379"
    networks:
      - fintech-network

  rabbitmq:
    image: rabbitmq:3-management-alpine
    container_name: fintech-rabbitmq
    ports:
      - "5672:5672"
      - "15672:15672"
    environment:
      RABBITMQ_DEFAULT_USER: ${RABBITMQ_USER:-guest}
      RABBITMQ_DEFAULT_PASS: ${RABBITMQ_PASSWORD:-guest}
    networks:
      - fintech-network

  fintech.api:
    build:
      context: .
      dockerfile: Fintech.Api/Dockerfile
    container_name: fintech-api
    ports:
      - "5140:8080"
    environment:
      ConnectionStrings__Mongo: mongodb://${MONGO_USER:-admin}:${MONGO_PASSWORD:-changeme}@mongodb:27017
      ConnectionStrings__Redis: redis:6379
      RabbitMQ__Host: rabbitmq
      Jwt__Secret: ${JWT_SECRET}
    depends_on:
      - mongodb
      - redis
      - rabbitmq
    networks:
      - fintech-network

  fintech.worker:
    build:
      context: .
      dockerfile: Fintech.Worker/Dockerfile
    container_name: fintech-worker
    environment:
      ConnectionStrings__Mongo: mongodb://${MONGO_USER:-admin}:${MONGO_PASSWORD:-changeme}@mongodb:27017
      RabbitMQ__Host: rabbitmq
    depends_on:
      - mongodb
      - rabbitmq
    networks:
      - fintech-network

volumes:
  mongodb_data:

networks:
  fintech-network:
    driver: bridge
```

---

### 19. Falta de Dockerfile para Worker
**Severidade:** 🟡 MÉDIO  
**Arquivo:** Não existe `Fintech.Worker/Dockerfile`

**Solução Recomendada:**
Criar `Fintech.Worker/Dockerfile`:
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Fintech.Worker/Fintech.Worker.csproj", "Fintech.Worker/"]
COPY ["Fintech.Entities/Fintech.Entities.csproj", "Fintech.Entities/"]
COPY ["Fintech.Interfaces/Fintech.Interfaces.csproj", "Fintech.Interfaces/"]
COPY ["Fintech.Messaging/Fintech.Messaging.csproj", "Fintech.Messaging/"]
COPY ["Fintech.Repositories/Fintech.Repositories.csproj", "Fintech.Repositories/"]
RUN dotnet restore "Fintech.Worker/Fintech.Worker.csproj"
COPY . .
WORKDIR "/src/Fintech.Worker"
RUN dotnet build "Fintech.Worker.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Fintech.Worker.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Fintech.Worker.dll"]
```

---

### 20. Terraform Sem Backend Remoto
**Severidade:** 🟡 MÉDIO  
**Arquivo:** `main.tf`

**Problema:**
- Nenhuma configuração de backend remoto
- Estado do Terraform local (não versionável)
- Risco de perda de estado

**Solução Recomendada:**
```terraform
terraform {
  required_version = ">= 1.0"
  
  backend "s3" {
    bucket         = "daibank-terraform-state"
    key            = "prod/terraform.tfstate"
    region         = "us-east-1"
    encrypt        = true
    dynamodb_table = "terraform-state-lock"
  }
  
  required_providers {
    aws = {
      source  = "hashicorp/aws"
      version = "~> 5.0"
    }
  }
}

# Usar variáveis para credenciais
variable "db_master_password" {
  description = "Master password for DocumentDB"
  type        = string
  sensitive   = true
}

variable "rabbitmq_password" {
  description = "Password for RabbitMQ"
  type        = string
  sensitive   = true
}

# Usar AWS Secrets Manager
resource "aws_secretsmanager_secret" "db_password" {
  name = "daibank/db/master-password"
}

resource "aws_secretsmanager_secret_version" "db_password" {
  secret_id     = aws_secretsmanager_secret.db_password.id
  secret_string = var.db_master_password
}
```

---

## 🟢 MELHORIAS RECOMENDADAS

### 21. Implementar Circuit Breaker
**Severidade:** 🟢 BAIXO  
**Justificativa:** Melhorar resiliência em chamadas externas

```csharp
builder.Services.AddHttpClient<IPixGateway, PixGateway>()
    .AddPolicyHandler(Policy
        .Handle<HttpRequestException>()
        .CircuitBreakerAsync(
            handledEventsAllowedBeforeBreaking: 3,
            durationOfBreak: TimeSpan.FromSeconds(30)
        ));
```

---

### 22. Adicionar Swagger com Autenticação
**Severidade:** 🟢 BAIXO  

```csharp
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "D.AI Bank API", Version = "v1" });
    
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});
```

---

### 23. Implementar Soft Delete para Auditoria
**Severidade:** 🟢 BAIXO  

```csharp
public abstract class AuditableEntity
{
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public Guid CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public Guid? UpdatedBy { get; set; }
    public DateTime? DeletedAt { get; set; }
    public Guid? DeletedBy { get; set; }
    public bool IsDeleted => DeletedAt.HasValue;
}
```

---

### 24. Adicionar Métricas Customizadas
**Severidade:** 🟢 BAIXO  

```csharp
public static class FintechMetrics
{
    private static readonly Histogram TransactionDuration = 
        Meter.CreateHistogram<double>("fintech_transaction_duration_seconds");
    
    private static readonly Counter PixTransactions = 
        Meter.CreateCounter<long>("fintech_pix_transactions_total");
    
    public static void RecordTransactionDuration(double seconds, string type)
    {
        TransactionDuration.Record(seconds, new KeyValuePair<string, object>("type", type));
    }
}
```

---

### 25. Implementar Feature Flags
**Severidade:** 🟢 BAIXO  

```csharp
builder.Services.AddFeatureManagement();

// Uso
if (await _featureManager.IsEnabledAsync("PixTransactions"))
{
    // Lógica de Pix
}
```

---

## 📊 Priorização de Correções

### Sprint 1 (Crítico - 1-2 semanas)
1. ✅ Remover credenciais hardcoded e implementar secrets management
2. ✅ Remover/proteger endpoint `/api/v1/transfer/setup`
3. ✅ Implementar validação de input em todos os endpoints
4. ✅ Adicionar rate limiting
5. ✅ Forçar HTTPS em produção

### Sprint 2 (Alto - 2-3 semanas)
6. ✅ Implementar proteção CSRF
7. ✅ Adicionar sanitização de logs
8. ✅ Completar auditoria no Ledger
9. ✅ Descomentar e corrigir transações MongoDB
10. ✅ Corrigir MongoContext para limpar sessão

### Sprint 3 (Médio - 3-4 semanas)
11. ✅ Melhorar middleware de idempotência
12. ✅ Corrigir PixOrchestrator para usar chave real
13. ✅ Adicionar timeouts em operações async
14. ✅ Implementar lock distribuído no OutboxWorker
15. ✅ Adicionar validação de moeda em Money
16. ✅ Criar índices MongoDB

### Sprint 4 (Baixo - Contínuo)
17. ✅ Adicionar health checks
18. ✅ Completar docker-compose
19. ✅ Criar Dockerfile do Worker
20. ✅ Configurar Terraform backend
21-25. ✅ Implementar melhorias (Circuit Breaker, Swagger, etc)

---

## 🔍 Checklist de Segurança para Deploy

### Antes de ir para Produção:
- [ ] Todas as credenciais foram removidas do código
- [ ] Secrets configurados em Azure Key Vault / AWS Secrets Manager
- [ ] HTTPS obrigatório em todos os endpoints
- [ ] Rate limiting configurado
- [ ] CORS configurado adequadamente
- [ ] Logs não expõem PII
- [ ] Todos os endpoints têm autenticação (exceto login/register)
- [ ] Validação de input em 100% dos endpoints
- [ ] Transações MongoDB em todos os fluxos críticos
- [ ] Índices MongoDB criados
- [ ] Health checks funcionando
- [ ] Monitoring e alertas configurados
- [ ] Backup automático do MongoDB
- [ ] Disaster recovery plan documentado
- [ ] Penetration testing realizado
- [ ] LGPD compliance verificado

---

## 📚 Referências e Recursos

### Segurança
- [OWASP Top 10](https://owasp.org/www-project-top-ten/)
- [OWASP API Security Top 10](https://owasp.org/www-project-api-security/)
- [CWE Top 25](https://cwe.mitre.org/top25/)

### .NET Security
- [Microsoft Security Best Practices](https://docs.microsoft.com/en-us/aspnet/core/security/)
- [OWASP .NET Security Cheat Sheet](https://cheatsheetseries.owasp.org/cheatsheets/DotNet_Security_Cheat_Sheet.html)

### MongoDB Security
- [MongoDB Security Checklist](https://docs.mongodb.com/manual/administration/security-checklist/)
- [MongoDB Encryption](https://docs.mongodb.com/manual/core/security-encryption-at-rest/)

### Compliance
- [LGPD - Lei Geral de Proteção de Dados](https://www.gov.br/cidadania/pt-br/acesso-a-informacao/lgpd)
- [PCI DSS](https://www.pcisecuritystandards.org/)
- [Banco Central - Regulamentação Pix](https://www.bcb.gov.br/estabilidadefinanceira/pix)

---

## 📞 Contato e Suporte

Para questões sobre este roadmap:
- **Analista:** Antigravity AI
- **Data:** 2026-01-20
- **Projeto:** D.AI Bank - Core Banking Engine

---

**Nota Final:** Este documento deve ser tratado como **CONFIDENCIAL** e contém informações sensíveis sobre vulnerabilidades de segurança. Distribua apenas para membros autorizados da equipe de desenvolvimento e segurança.
