using Fintech.Core.Interfaces;
using Fintech.Enums;

namespace Fintech.Entities;

public class PixSaga : IMultiTenant
{
    public Guid Id { get; private set; }
    public Guid TenantId { get; private set; }
    public Guid AccountId { get; private set; }
    public decimal Amount { get; private set; }
    public PixStatus Status { get; private set; }
    public string? FailureReason { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public string PixKey { get; set; }

    public PixSaga(Guid accountId, Guid tenantId, decimal amount)
    {
        Id = Guid.NewGuid();
        AccountId = accountId;
        TenantId = tenantId;
        Amount = amount;
        Status = PixStatus.Created;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkAsLocked() => SetStatus(PixStatus.BalanceLocked);
    public void MarkAsCompleted() => SetStatus(PixStatus.Completed);
    public void MarkAsFailed(string reason) { FailureReason = reason; SetStatus(PixStatus.Failed); }
    public void MarkAsRefunded() => SetStatus(PixStatus.Refunded);

    private void SetStatus(PixStatus s) { Status = s; UpdatedAt = DateTime.UtcNow; }
}