using Fintech.Interfaces;
using Fintech.Core.Interfaces;
using System.Linq;

namespace Fintech.Services;

public class OpenBankingService : IOpenBankingService
{
    private readonly IAccountRepository _accountRepo;
    private readonly ILedgerRepository _ledgerRepo;

    public OpenBankingService(IAccountRepository accountRepo, ILedgerRepository ledgerRepo)
    {
        _accountRepo = accountRepo;
        _ledgerRepo = ledgerRepo;
    }


    public async Task<IEnumerable<AccountDto>> GetAccountsAsync()
    {
        // Em um cenário real, aqui filtraríamos as contas que o usuário deu consentimento
        var accounts = await _accountRepo.GetAllAsync(); // Idealmente filtrado
        return accounts.Select(a => new AccountDto(a.Id, "CHECKING", "BRL"));
    }

    public async Task<BalanceDto> GetBalanceAsync(Guid accountId)
    {
        var account = await _accountRepo.GetByIdAsync(accountId);
        if (account == null) throw new KeyNotFoundException("Account not found");

        var balance = account.Balances.FirstOrDefault().Value; // Simplificado
        return new BalanceDto(balance.Amount, balance.Currency.Code);
    }

    public async Task<IEnumerable<TransactionDto>> GetTransactionsAsync(Guid accountId)
    {
        var ledgerEvents = await _ledgerRepo.GetByAccountIdAsync(accountId);
        return ledgerEvents.Select(e => new TransactionDto(e.Id, e.Type, e.Amount, e.Timestamp));
    }
}
