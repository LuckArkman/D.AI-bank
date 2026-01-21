namespace Fintech.Interfaces;

public interface IBusinessRulesEngine
{
    Task<RuleExecutionResult> EvaluateAsync(string expression, object context);
}

public record RuleExecutionResult(bool Success, List<string> Errors, List<string> Warnings);
