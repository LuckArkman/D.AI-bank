using Fintech.Entities;
using Fintech.Interfaces;
using Fintech.Repositories;
using Fintech.ValueObjects;

namespace Fintech.Application.Commands;

public class CreateAccountHandler
{
    readonly AccountRepository _accountRepo;
    private readonly ITransactionManager _txManager;
    readonly LedgerRepository _ledgerRepo;

    public async Task<Guid> Handle(decimal initialBalance)
    {
        // Criação de conta é crítica, também deve ser ACID
        using var uow = await _txManager.BeginTransactionAsync();
        try
        {
            var accountId = Guid.NewGuid();
            var account = new Account(accountId, initialBalance: 0.00m);
            
            // Simula um depósito inicial (Bonus de abertura)
            if (initialBalance > 0)
            {
                // Aqui precisaríamos de um método Credit() na entidade, 
                // ou apenas manipulamos o dicionário diretamente na criação
                // Para simplificar, vamos assumir que o construtor ou método factory cuida disso
                // ou injetamos saldo inicial via "Credit" logic
            }

            // Persiste
            await _accountRepo.AddAsync(account); // *Precisamos adicionar este método no Repo*
            
            // Ledger Inicial
            var ledger = new LedgerEvent 
            { 
                AccountId = accountId, 
                Type = "ACCOUNT_CREATED", 
                Amount = initialBalance,
                // ...
            };
            await _ledgerRepo.AddAsync(ledger);

            await uow.CommitAsync();
            return accountId;
        }
        catch
        {
            throw;
        }
    }
}