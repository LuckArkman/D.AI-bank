namespace Fintech.IntegrationTests;

using Testcontainers.MongoDb;
using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Fintech.Persistence;
using MongoDB.Driver;
using Fintech.Commands;

public class TransactionTests : IAsyncLifetime
{
    private readonly MongoDbContainer _mongo = new MongoDbBuilder().Build();
    private IServiceProvider _sp;

    public async Task InitializeAsync()
    {
        await _mongo.StartAsync();
        var services = new ServiceCollection();
        services.AddSingleton<IMongoClient>(new MongoClient(_mongo.GetConnectionString()));
        services.AddScoped<MongoContext>();
        services.AddScoped<Fintech.Core.Interfaces.ITransactionManager>(sp => sp.GetRequiredService<MongoContext>());
        // Registrar outros serviços...
        _sp = services.BuildServiceProvider();
    }

    public Task DisposeAsync() => _mongo.DisposeAsync();

    [Fact]
    public async Task Rollback_Deve_Funcionar_Se_Erro_Ocorrer()
    {
        using var scope = _sp.CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<DebitAccountHandler>();
        
        // Simular erro forçado (Mock ou Exception proposital)
        // Assert que o saldo no banco continua intacto
    }
}