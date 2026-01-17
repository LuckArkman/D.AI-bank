namespace Fintech.Entities;

public class Account
{
    public Guid Id { get; set; } // AccountId
    public decimal Balances { get; private set; }
    public long Version { get; private set; } // Optimistic Concurrency Control
    public DateTime LastUpdated { get; private set; }

    // Construtor para ORM
    public Account(Guid id, decimal initialBalance) 
    {
        Id = id;
        Balances = initialBalance;
        Version = 1;
        LastUpdated = DateTime.UtcNow;
    }
}