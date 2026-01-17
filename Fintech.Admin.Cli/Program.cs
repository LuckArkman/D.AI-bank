using Fintech.Admin.Cli;
using Microsoft.Extensions.DependencyInjection;
using Fintech.Commands;
using Fintech.Interfaces;
using Fintech.Persistence;
using MongoDB.Driver;

var services = new ServiceCollection();
services.AddSingleton<IMongoClient>(new MongoClient("mongodb://mplopes:3702959@localhost:27017"));
services.AddScoped<MongoContext>();
services.AddScoped<ITransactionManager>(sp => sp.GetRequiredService<MongoContext>());
services.AddScoped<DebitAccountHandler>();

var sp = services.BuildServiceProvider();
var command = args[0]; // "credit-manual"

if (command == "credit-manual")
{
    var accountId = Guid.Parse(args[1]);
    var amount = decimal.Parse(args[2]);
    
    // Aqui chamaríamos um Handler Administrativo específico que exige "Reason"
    Console.WriteLine($"Ajuste manual executado na conta {accountId}");
}
// Adicione no Parser.Default.ParseArguments<..., SeedCommand>(args)
// .WithParsed<SeedCommand>(cmd => SeedData(cmd, provider));

static async Task SeedData(SeedCommand cmd, IServiceProvider sp)
{
    var createHandler = sp.GetRequiredService<CreateAccountHandler>();
    var debitHandler = sp.GetRequiredService<DebitAccountHandler>();
    var random = new Random();

    Console.WriteLine($"🌱 Criando {cmd.Count} contas com transações...");

    for (int i = 0; i < cmd.Count; i++)
    {
        // 1. Cria Conta com Saldo Inicial randomico
        var initialBalance = random.Next(1000, 50000);
        var accId = await createHandler.Handle(initialBalance);

        Console.Write($"."); // Feedback visual

        // 2. Gera 5 a 10 transações de débito aleatórias
        var txCount = random.Next(5, 10);
        for (int j = 0; j < txCount; j++)
        {
            var amount = random.Next(10, 500);
            try
            {
                await debitHandler.Handle(accId, amount, Guid.NewGuid());
            }
            catch { /* Ignora erro de saldo em seed */ }
        }
    }
    Console.WriteLine("\n✅ Seed concluído.");
}