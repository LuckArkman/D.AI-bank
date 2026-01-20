namespace Fintech.Regulatory.Rules;

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
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ConditionExpression { get; set; } = string.Empty; // e.g., "amount > 1000 && hour > 20"
    public string ErrorMessage { get; set; } = string.Empty;
    public RuleSeverity Severity { get; set; } = RuleSeverity.Error;
}

public interface IBusinessRulesEngine
{
    Task<RuleExecutionResult> EvaluateAsync(string ruleSetId, object context);
}

public record RuleExecutionResult(bool Success, List<string> Errors, List<string> Warnings);
