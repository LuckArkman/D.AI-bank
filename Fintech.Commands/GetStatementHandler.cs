using MongoDB.Driver;
using Fintech.Entities;
using Fintech.Records;
using Fintech.Repositories;

namespace Fintech.Commands;

public class GetStatementHandler
{
    private readonly AccountRepository _accountRepo;
    private readonly IMongoCollection<LedgerEvent> _ledgerCollection;

    public GetStatementHandler(IMongoClient client, AccountRepository accountRepo)
    {
        _accountRepo = accountRepo;
        var database = client.GetDatabase("FintechDB");
        _ledgerCollection = database.GetCollection<LedgerEvent>("ledger");
    }

    public async Task<StatementResponse> Handle(Guid accountId)
    {
        // 1. Busca o Saldo Atual (Consistente)
        var account = await _accountRepo.GetByIdAsync(accountId);
        
        // Assume BRL como padrão, ou soma todas as moedas se preferir
        var currentBalance = account.Balances.ContainsKey("BRL") ? account.Balances["BRL"].Amount : 0;

        // 2. Busca as últimas 20 transações no Ledger (Leitura Otimizada)
        var filter = Builders<LedgerEvent>.Filter.Eq(x => x.AccountId, accountId);
        
        var events = await _ledgerCollection.Find(filter)
            .SortByDescending(x => x.Timestamp)
            .Limit(20)
            .ToListAsync();

        // 3. Mapeia para DTO
        var items = events.Select(e => new StatementItem(
            e.Timestamp,
            e.Type,
            e.Amount,
            e.CorrelationId.ToString()
        )).ToList();

        return new StatementResponse(currentBalance, items);
    }
}