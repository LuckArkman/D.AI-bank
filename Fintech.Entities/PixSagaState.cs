namespace Fintech.Entities;

public class PixSagaState
{
    public Guid Id { get; set; } // SagaId (CorrelationId)
    public Guid AccountId { get; set; }
    public decimal Amount { get; set; }
    public string PixKey { get; set; }
    public string CurrentState { get; set; } // Created, BalanceLocked, PixSent, Completed, Failed
    public string FailureReason { get; set; }
    public DateTime UpdatedAt { get; set; }
}