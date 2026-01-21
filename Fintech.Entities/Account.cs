using Fintech.ValueObjects;
using Fintech.Enums;

using Fintech.Core.Interfaces;

namespace Fintech.Entities;

public class Account : IMultiTenant
{
    public Guid Id { get; private set; }
    public Guid TenantId { get; private set; }
    public AccountProfileType ProfileType { get; private set; }
    // Correção: Alterado de decimal para Dictionary para suportar Money e indexação ["BRL"]
    public Dictionary<string, Money> Balances { get; private set; } = new();
    public long Version { get; private set; } // Optimistic Concurrency Control
    public DateTime LastUpdated { get; private set; }

    public Account(Guid id, Guid tenantId, AccountProfileType profileType = AccountProfileType.StandardIndividual)
    {
        Id = id;
        TenantId = tenantId;
        ProfileType = profileType;
        // Inicializa com zero BRL

        Balances["BRL"] = Money.BRL(0);
        Version = 1;
        LastUpdated = DateTime.UtcNow;
    }

    // Correção: O método agora aceita o ValueObject Money
    public void Debit(Money amount)
    {
        if (!Balances.ContainsKey(amount.Currency.Code))
            throw new InvalidOperationException("Conta não possui saldo nesta moeda.");

        var currentBalance = Balances[amount.Currency.Code];

        if (currentBalance < amount)
            throw new InvalidOperationException("Saldo insuficiente.");

        // Atualiza o saldo
        Balances[amount.Currency.Code] = currentBalance - amount;

        LastUpdated = DateTime.UtcNow;
    }

    public void Credit(Money amount)
    {
        // Se a moeda não existe, inicializa com zero daquela moeda
        if (!Balances.ContainsKey(amount.Currency.Code))
        {
            Balances[amount.Currency.Code] = Money.Create(0, amount.Currency.Code);
        }

        Balances[amount.Currency.Code] = Balances[amount.Currency.Code] + amount;
        LastUpdated = DateTime.UtcNow;
    }
}