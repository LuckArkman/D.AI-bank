using Fintech.ValueObjects;

namespace Fintech.Entities;

public class Account
{
    public Guid Id { get; private set; }
    // Correção: Alterado de decimal para Dictionary para suportar Money e indexação ["BRL"]
    public Dictionary<string, Money> Balances { get; private set; } = new();
    public long Version { get; private set; } // Optimistic Concurrency Control
    public DateTime LastUpdated { get; private set; }

    public Account(Guid id)
    {
        Id = id;
        // Inicializa com zero BRL
        Balances["BRL"] = Money.BRL(0);
        Version = 1;
        LastUpdated = DateTime.UtcNow;
    }

    // Correção: O método agora aceita o ValueObject Money
    public void Debit(Money amount)
    {
        if (!Balances.ContainsKey(amount.Currency))
            throw new InvalidOperationException("Conta não possui saldo nesta moeda.");

        var currentBalance = Balances[amount.Currency];

        if (currentBalance < amount)
            throw new InvalidOperationException("Saldo insuficiente.");

        // Atualiza o saldo
        Balances[amount.Currency] = currentBalance - amount;
        
        LastUpdated = DateTime.UtcNow;
    }

    public void Credit(Money amount)
    {
        if (!Balances.ContainsKey(amount.Currency))
        {
            Balances[amount.Currency] = Money.BRL(0);
        }

        Balances[amount.Currency] = Balances[amount.Currency] + amount;
        LastUpdated = DateTime.UtcNow;
    }
}