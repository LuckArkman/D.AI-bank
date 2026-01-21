namespace Fintech.Entities;

public enum RuleSeverity
{
    Information,
    Warning,
    Error,
    Blocking
}

public class BusinessRule
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public Guid TenantId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ConditionExpression { get; set; } = string.Empty; // e.g., "amount > 1000 && hour > 20"
    public string ErrorMessage { get; set; } = string.Empty;
    public RuleSeverity Severity { get; set; } = RuleSeverity.Error;
}
