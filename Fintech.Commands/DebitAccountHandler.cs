using Fintech.Entities;
using Fintech.Interfaces;
using Fintech.ValueObjects;

namespace Fintech.Commands;

public class DebitAccountHandler
{
    private readonly ITransactionManager _txManager;
    // Assumindo IAccountRepository e IOutboxRepository injetados via construtor

    public async Task Handle(Guid accountId, decimal amount, Guid correlationId)
    {
        using var uow = await _txManager.BeginTransactionAsync();
        try
        {
            // 1. Carrega (exemplo simplificado)
            var account = await _repo.GetByIdAsync(accountId); // Repository deve usar _context.Session
            
            // 2. Domínio
            account.Debit(Money.BRL(amount));
            
            // 3. Persiste Estado
            await _repo.UpdateAsync(account); // Usa Optimistic Concurrency no filtro

            // 4. Persiste Outbox (Mesma Transação)
            var msg = new OutboxMessage("AccountDebited", new { AccountId = accountId, Amount = amount });
            await _outboxRepo.AddAsync(msg);

            // 5. Commit
            await uow.CommitAsync();
        }
        catch
        {
            // Auto-rollback no dispose
            throw;
        }
    }
}