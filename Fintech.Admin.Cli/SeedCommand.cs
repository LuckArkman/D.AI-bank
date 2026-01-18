using CommandLine;

namespace Fintech.Admin.Cli;

[Verb("seed", HelpText = "Popula o banco com dados de teste.")]
public class SeedCommand
{
    [Option('c', "count", Default = 10, HelpText = "Número de contas a criar.")]
    public int Count { get; set; }
}