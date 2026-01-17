using Fintech.Persistence;
using Fintech.Messaging;
using Fintech.Repositories;
using Fintech.Interfaces;
using Fintech.Worker;
using MongoDB.Driver;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

var builder = Host.CreateApplicationBuilder(args);

// 1. Configuração do MongoDB (Mesma da API)
MongoClassMaps.Register();
builder.Services.AddSingleton<IMongoClient>(sp => 
    new MongoClient(builder.Configuration.GetConnectionString("Mongo")));
builder.Services.AddScoped<MongoContext>();
builder.Services.AddScoped<ITransactionManager>(sp => sp.GetRequiredService<MongoContext>());

// 2. Repositórios (Precisam estar aqui também, pois o Worker usa para ler o Outbox e Atualizar Sagas)
builder.Services.AddScoped<IOutboxRepository, OutboxRepository>();
builder.Services.AddScoped<SagaRepository>();
builder.Services.AddScoped<AccountRepository>(); // Para compensações (Refund)
builder.Services.AddScoped<LedgerRepository>();

// 3. Mensageria
builder.Services.AddSingleton<IMessageBus, RabbitMqClient>();

// 4. Background Services (Os Workers reais)
// O Dispatcher que lê o Mongo e joga no Rabbit
builder.Services.AddHostedService<OutboxDispatcher>();
// O Arquivador noturno
builder.Services.AddHostedService<LedgerArchiver>();
// O Consumidor de filas (Pix) - Normalmente seria um IHostedService encapsulando o RabbitMQ Consumer
// Aqui simplificado:
// builder.Services.AddHostedService<PixConsumerWorker>(); 

// 5. Observabilidade (OpenTelemetry para Worker)
builder.Services.AddOpenTelemetry()
    .WithTracing(tracing =>
    {
        tracing
            .SetResourceBuilder(
                ResourceBuilder.CreateDefault()
                    .AddService("Fintech.Worker")
            )
            .AddSource("Fintech.MongoDB")
            .AddSource("Fintech.Worker")
            .AddOtlpExporter(opt =>
            {
                opt.Endpoint = new Uri("http://localhost:4317");
            });
    });


var host = builder.Build();
host.Run();