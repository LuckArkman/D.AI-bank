using Fintech.Interfaces;
using Fintech.Core.Interfaces;
using System.Linq;

namespace Fintech.Services;

public class AdminService : IAdminService
{
    private readonly IAccountRepository _accountRepo;
    private readonly IOutboxRepository _outboxRepo;
    private readonly ILedgerRepository _ledgerRepo;

    public AdminService(IAccountRepository accountRepo, IOutboxRepository outboxRepo, ILedgerRepository ledgerRepo)
    {
        _accountRepo = accountRepo;
        _outboxRepo = outboxRepo;
        _ledgerRepo = ledgerRepo;
    }


    public async Task<SystemStatsDto> GetSystemStatsAsync()
    {
        var accounts = await _accountRepo.GetAllAsync();
        var pendingMessages = await _outboxRepo.GetPendingAsync(100);

        var totalBalance = accounts.Sum(a => a.Balances.Values.Sum(v => v.Amount));

        return new SystemStatsDto(accounts.Count(), totalBalance, pendingMessages.Count());
    }

    public async Task<IEnumerable<AuditLogDto>> GetAuditLogsAsync()
    {
        // Usando o Ledger como uma trilha de auditoria inicial
        var events = await _ledgerRepo.GetAllAsync(); // Idealmente filtrado por tipo
        return events.Select(e => new AuditLogDto(e.Id, e.Type, e.AccountId.ToString(), e.Timestamp));
    }
}
