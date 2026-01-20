using Fintech.Entities;
using Fintech.Records;
using Fintech.Core.Interfaces;
using System.Linq;

namespace Fintech.Commands;

public class GetStatementHandler
{
    private readonly IAccountRepository _accountRepo;
    private readonly ILedgerRepository _ledgerRepo;

    public GetStatementHandler(IAccountRepository accountRepo, ILedgerRepository ledgerRepo)
    {
        _accountRepo = accountRepo;
        _ledgerRepo = ledgerRepo;
    }

    public async Task<StatementResponse> Handle(Guid accountId)
    {
        // 1. Busca o Saldo Atual (Consistente)
        var account = await _accountRepo.GetByIdAsync(accountId);

        // Assume BRL como padrão
        var currentBalance = account.Balances.ContainsKey("BRL") ? account.Balances["BRL"].Amount : 0;

        // 2. Busca as transações no Ledger via repositório
        var events = await _ledgerRepo.GetByAccountIdAsync(accountId);

        // 3. Mapeia para DTO (Ordenando e limitando em memória para simplificar o repositório genérico)
        var items = events
            .OrderByDescending(x => x.Timestamp)
            .Take(20)
            .Select(e => new StatementItem(
                e.Timestamp,
                e.Type,
                e.Amount,
                e.CorrelationId.ToString()
            )).ToList();

        return new StatementResponse(currentBalance, items);
    }
}
