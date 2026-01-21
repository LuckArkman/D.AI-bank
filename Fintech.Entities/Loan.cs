using Fintech.Core.Interfaces;
using Fintech.Enums;

namespace Fintech.Entities;

public class Loan : IMultiTenant
{
    public Guid Id { get; private set; }
    public Guid TenantId { get; private set; }
    public Guid AccountId { get; private set; }
    public decimal PrincipalAmount { get; private set; }
    public decimal InterestRate { get; private set; }
    public int Installments { get; private set; }
    public decimal MonthlyPayment { get; private set; }
    public decimal TotalToPay { get; private set; }
    public decimal RemainingBalance { get; private set; }
    public LoanStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? ApprovedAt { get; private set; }

    public Loan(Guid accountId, Guid tenantId, decimal principalAmount, decimal interestRate, int installments)
    {
        Id = Guid.NewGuid();
        AccountId = accountId;
        TenantId = tenantId;
        PrincipalAmount = principalAmount;
        InterestRate = interestRate;
        Installments = installments;

        // Simulação básica de cálculo de juros
        TotalToPay = principalAmount * (1 + (interestRate / 100));
        MonthlyPayment = TotalToPay / installments;
        RemainingBalance = TotalToPay;
        Status = LoanStatus.Pending;
        CreatedAt = DateTime.UtcNow;
    }

    public void Approve()
    {
        Status = LoanStatus.Active;
        ApprovedAt = DateTime.UtcNow;
    }

    public void MarkPaid() => Status = LoanStatus.Paid;
}
