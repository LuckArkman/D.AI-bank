using Microsoft.Extensions.DependencyInjection;
using Fintech.Commands;
using Fintech.Interfaces;
using Fintech.Persistence;
using MongoDB.Driver;

// Setup Mínimo de DI para Console
var services = new ServiceCollection();
services.AddSingleton<IMongoClient>(new MongoClient("mongodb://localhost:27017"));
services.AddScoped<MongoContext>();
services.AddScoped<ITransactionManager>(sp => sp.GetRequiredService<MongoContext>());
// Adicionar Repositories...
services.AddScoped<DebitAccountHandler>();

var sp = services.BuildServiceProvider();

// Parser de argumentos (simplificado)
var command = args[0]; // "credit-manual"

if (command == "credit-manual")
{
    var accountId = Guid.Parse(args[1]);
    var amount = decimal.Parse(args[2]);
    
    // Aqui chamaríamos um Handler Administrativo específico que exige "Reason"
    Console.WriteLine($"Ajuste manual executado na conta {accountId}");
}