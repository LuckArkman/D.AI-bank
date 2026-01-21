using DynamicExpresso;
using Fintech.Interfaces;

namespace Fintech.Regulatory.Rules;

public class BusinessRulesEngine : IBusinessRulesEngine
{
    private readonly Interpreter _interpreter;

    public BusinessRulesEngine()
    {
        _interpreter = new Interpreter();
    }

    public Task<RuleExecutionResult> EvaluateAsync(string expression, object context)
    {
        var errors = new List<string>();
        var warnings = new List<string>();

        try
        {
            // Inject context properties as variables for the expression
            var result = _interpreter.Eval<bool>(expression, new Parameter("ctx", context));

            return Task.FromResult(new RuleExecutionResult(result, errors, warnings));
        }
        catch (Exception ex)
        {
            errors.Add($"Error evaluating rule: {ex.Message}");
            return Task.FromResult(new RuleExecutionResult(false, errors, warnings));
        }
    }
}
