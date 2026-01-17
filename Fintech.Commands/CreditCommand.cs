using CommandLine;

namespace Fintech.Admin.Cli;

[Verb("credit-manual", HelpText = "Realiza um crédito administrativo em uma conta.")]
public class CreditCommand
{
    [Option('i', "id", Required = true, HelpText = "O ID da Conta (GUID).")]
    public string AccountId { get; set; } = string.Empty;

    [Option('a', "amount", Required = true, HelpText = "O valor a ser creditado.")]
    public decimal Amount { get; set; }

    [Option('r', "reason", Required = false, HelpText = "Motivo do crédito (Auditoria).")]
    public string? Reason { get; set; }
}