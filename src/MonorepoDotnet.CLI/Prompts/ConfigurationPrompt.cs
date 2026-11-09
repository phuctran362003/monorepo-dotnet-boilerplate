using MonorepoDotnet.Core.Generation;
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

        var normalizedName = SolutionNameValidator.Normalize(solutionName);

        var outputDirectory = AnsiConsole.Prompt(
            new TextPrompt<string>("Output [green]directory[/]:")
                .DefaultValue(Environment.CurrentDirectory)
                .PromptStyle("yellow")
                .Validate(value =>
                {
                    var errors = OutputPathValidator.ValidateTarget(value, normalizedName);
                    return errors.Count == 0
                        ? ValidationResult.Success()
                        : ValidationResult.Error(string.Join(" ", errors));
                }));

        var dotNetVersion = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Target [green].NET version[/]:")
                .AddChoices(DotNetVersionCatalog.All.Select(profile => profile.ChoiceLabel)));

        var architecture = AnsiConsole.Prompt(
            new SelectionPrompt<ArchitectureType>()
                .Title("[green]Architecture[/]:")
                .AddChoices(ArchitectureTypeExtensions.All())
                .UseConverter(candidate => candidate.ToDisplayName()));

        var database = AnsiConsole.Prompt(
            new SelectionPrompt<DatabaseProvider>()
                .Title("[green]Database[/]:")
                .AddChoices(DatabaseProviderExtensions.All())
                .UseConverter(provider => provider.ToDisplayName()));

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
            SolutionName = normalizedName,
            OutputDirectory = OutputPathValidator.Normalize(outputDirectory),
            DotNetVersion = dotNetVersion.Split(' ')[0],
            Architecture = architecture,
            Database = database,
            IncludeDocker = includeDocker,
            Packages = packages.Aggregate(OptionalPackage.None, (current, package) => current | package),
            BuildAfterGeneration = true
        };
    }
}
