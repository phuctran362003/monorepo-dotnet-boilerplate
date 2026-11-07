using MonorepoDotnet.Core.Models;
using MonorepoDotnet.Core.Validation;
using Spectre.Console;

namespace MonorepoDotnet.CLI.Prompts;

/// <summary>Collects the solution configuration interactively.</summary>
public sealed class ConfigurationPrompt
{
    public GeneratorOptions Prompt()
    {
        AnsiConsole.Write(new Rule("[bold cyan]Solution configuration[/]").LeftJustified());
        AnsiConsole.WriteLine();

        var solutionName = AnsiConsole.Prompt(
            new TextPrompt<string>("Solution [green]name[/]:")
                .PromptStyle("yellow")
                .Validate(value =>
                {
                    var errors = SolutionNameValidator.Validate(value);
                    return errors.Count == 0
                        ? ValidationResult.Success()
                        : ValidationResult.Error(string.Join(" ", errors));
                }));

        var outputDirectory = AnsiConsole.Prompt(
            new TextPrompt<string>("Output [green]directory[/]:")
                .DefaultValue(Environment.CurrentDirectory)
                .PromptStyle("yellow")
                .Validate(value => string.IsNullOrWhiteSpace(value)
                    ? ValidationResult.Error("Output directory is required.")
                    : ValidationResult.Success()));

        var dotNetVersion = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Target [green].NET version[/]:")
                .AddChoices("net10.0 (.NET 10)"));

        var architecture = AnsiConsole.Prompt(
            new SelectionPrompt<ArchitectureType>()
                .Title("[green]Architecture[/]:")
                .AddChoices(ArchitectureType.Ddd)
                .UseConverter(architecture => architecture switch
                {
                    ArchitectureType.Ddd => "DDD (Domain-Driven Design)",
                    _ => architecture.ToString()
                }));

        var database = AnsiConsole.Prompt(
            new SelectionPrompt<DatabaseProvider>()
                .Title("[green]Database[/]:")
                .AddChoices(DatabaseProvider.PostgreSql)
                .UseConverter(provider => provider switch
                {
                    DatabaseProvider.PostgreSql => "PostgreSQL (EF Core, Code First)",
                    _ => provider.ToString()
                }));

        var includeDocker = AnsiConsole.Confirm(
            "Generate [green]Docker[/] files (Dockerfile, docker-compose.yml, .env.example)?",
            defaultValue: true);

        var packages = AnsiConsole.Prompt(
            new MultiSelectionPrompt<OptionalPackage>()
                .Title("Optional [green]packages[/] (space to select, enter to confirm):")
                .NotRequired()
                .PageSize(10)
                .InstructionsText("[grey](press [blue]<space>[/] to toggle, [green]<enter>[/] to accept)[/]")
                .AddChoices(OptionalPackageExtensions.All())
                .UseConverter(package => package.ToDisplayName()));

        return new GeneratorOptions
        {
            SolutionName = SolutionNameValidator.Normalize(solutionName),
            OutputDirectory = Path.GetFullPath(outputDirectory.Trim()),
            DotNetVersion = dotNetVersion.Split(' ')[0],
            Architecture = architecture,
            Database = database,
            IncludeDocker = includeDocker,
            Packages = packages.Aggregate(OptionalPackage.None, (current, package) => current | package),
            BuildAfterGeneration = true
        };
    }
}
