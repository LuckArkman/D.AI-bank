using Fintech.Core.Interfaces;
using Fintech.Enums;

namespace Fintech.Entities;

public class Investment : IMultiTenant
{
    public Guid Id { get; private set; }
    public Guid TenantId { get; private set; }
    public Guid AccountId { get; private set; }
    public string Name { get; private set; }
    public InvestmentType Type { get; private set; }
    public decimal PrincipalAmount { get; private set; }
    public decimal CurrentAmount { get; private set; }
    public decimal ExpectedReturnRate { get; private set; } // Anual %
    public DateTime CreatedAt { get; private set; }
    public DateTime? LiquidatedAt { get; private set; }

    public Investment(Guid accountId, Guid tenantId, string name, InvestmentType type, decimal principalAmount, decimal expectedReturnRate)
    {
        Id = Guid.NewGuid();
        AccountId = accountId;
        TenantId = tenantId;
        Name = name;
        Type = type;
        PrincipalAmount = principalAmount;
        CurrentAmount = principalAmount;
        ExpectedReturnRate = expectedReturnRate;
        CreatedAt = DateTime.UtcNow;
    }

    public void Updatevaluation(decimal newAmount)
    {
        CurrentAmount = newAmount;
    }

    public void Liquidate()
    {
        LiquidatedAt = DateTime.UtcNow;
    }
}

public class SavingsGoal : IMultiTenant
{
    public Guid Id { get; private set; }
    public Guid TenantId { get; private set; }
    public Guid AccountId { get; private set; }
    public string Name { get; private set; }
    public decimal TargetAmount { get; private set; }
    public decimal CurrentAmount { get; private set; }
    public string Color { get; private set; } // Hex or Tailwing class
    public GoalStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public SavingsGoal(Guid accountId, Guid tenantId, string name, decimal targetAmount, string color)
    {
        Id = Guid.NewGuid();
        AccountId = accountId;
        TenantId = tenantId;
        Name = name;
        TargetAmount = targetAmount;
        CurrentAmount = 0;
        Color = color;
        Status = GoalStatus.InProgress;
        CreatedAt = DateTime.UtcNow;
    }

    public void AddFunds(decimal amount)
    {
        CurrentAmount += amount;
        if (CurrentAmount >= TargetAmount)
            Status = GoalStatus.Completed;
    }

    public void WithdrawFunds(decimal amount)
    {
        if (amount > CurrentAmount) throw new InvalidOperationException("Saldo insuficiente na caixinha.");
        CurrentAmount -= amount;
        if (CurrentAmount < TargetAmount && Status == GoalStatus.Completed)
            Status = GoalStatus.InProgress;
    }
}
