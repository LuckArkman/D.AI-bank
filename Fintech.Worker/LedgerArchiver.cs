using Microsoft.Extensions.Hosting;
using MongoDB.Driver;
using Fintech.Entities;

namespace Fintech.Worker;

public class LedgerArchiver : BackgroundService
{
    private readonly IMongoCollection<LedgerEvent> _hot;
    private readonly IMongoCollection<LedgerEvent> _cold;
    private readonly IMongoClient _client;

    public LedgerArchiver(IMongoClient client)
    {
        _client = client;
        var db = _client.GetDatabase("FintechDB");
        _hot = db.GetCollection<LedgerEvent>("ledger");
        _cold = db.GetCollection<LedgerEvent>("ledger_archive");
    }

    protected override async Task ExecuteAsync(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            if (DateTime.UtcNow.Hour == 3) // Roda às 03:00 AM
            {
                var cutoff = DateTime.UtcNow.AddMonths(-6);
                var filter = Builders<LedgerEvent>.Filter.Lt(x => x.Timestamp, cutoff);
                
                // Lógica simplificada de mover e deletar
                // Em prod: fazer em batches com Transaction
            }
            await Task.Delay(TimeSpan.FromHours(1), token);
        }
    }
}