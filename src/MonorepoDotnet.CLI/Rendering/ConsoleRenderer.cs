using MonorepoDotnet.Core.Models;
using Spectre.Console;

namespace MonorepoDotnet.CLI.Rendering;

/// <summary>Shared Spectre.Console renderers.</summary>
public static class ConsoleRenderer
{
    public static void RenderBanner()
    {
        AnsiConsole.Write(new FigletText("monorepo-dotnet").Color(Color.Cyan1));
        AnsiConsole.MarkupLine("[grey]ASP.NET Core solution generator — DDD, Clean, 1-Layer and MVC — PostgreSQL, EF Core, Docker[/]");
        AnsiConsole.WriteLine();
    }

    public static void RenderConfiguration(GeneratorOptions options)
    {
        var table = new Table()
            .Border(TableBorder.Rounded)
            .BorderColor(Color.Grey)
            .Title("[bold]Configuration summary[/]")
            .AddColumn("Setting")
            .AddColumn("Value");

        table.AddRow("Solution name", Markup.Escape(options.SolutionName));
        table.AddRow("Output directory", Markup.Escape(options.OutputDirectory));
        table.AddRow("Target framework", Markup.Escape(options.DotNetVersion));
        table.AddRow("Architecture", Markup.Escape(options.Architecture.ToDisplayName()));
        table.AddRow("Database", Markup.Escape(options.Database.ToDisplayName()));
        table.AddRow("Docker", options.IncludeDocker ? "Yes" : "No");
        table.AddRow("Optional packages", FormatPackages(options.Packages));

        AnsiConsole.Write(table);
        AnsiConsole.WriteLine();
    }

    public static void RenderEnvironmentReport(EnvironmentReport report)
    {
        var table = new Table()
            .Border(TableBorder.Rounded)
            .BorderColor(Color.Grey)
            .Title("[bold]Environment checks[/]")
            .AddColumn("Check")
            .AddColumn("Status")
            .AddColumn("Details");

        foreach (var check in report.Checks)
        {
            var status = check.Severity switch
            {
                CheckSeverity.Error => "[red]Error[/]",
                CheckSeverity.Warning => "[yellow]Warning[/]",
                _ => "[green]OK[/]"
            };

            table.AddRow(Markup.Escape(check.Name), status, Markup.Escape(check.Message));
        }

        AnsiConsole.Write(table);
        AnsiConsole.WriteLine();
    }

    public static void RenderSuccess(GenerationResult result, GeneratorOptions options)
    {
        var panel = new Panel(
                new Markup(
                    $"[green]Solution generated successfully.[/]{Environment.NewLine}{Environment.NewLine}" +
                    $"Location: [blue]{Markup.Escape(result.RootDirectory)}[/]"))
            .Header("[green]Done[/]")
            .Border(BoxBorder.Rounded)
            .BorderColor(Color.Green);

        AnsiConsole.Write(panel);

        foreach (var warning in result.Warnings)
        {
            AnsiConsole.MarkupLine($"[yellow]Warning:[/] {Markup.Escape(warning)}");
        }

        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[grey]Next steps:[/]");
        AnsiConsole.MarkupLine($"  [grey]cd[/] {Markup.Escape(result.RootDirectory)}");
        AnsiConsole.MarkupLine("  [grey]dotnet build[/]");

        if (options.IncludeDocker)
        {
            AnsiConsole.MarkupLine("  [grey]docker compose up --build[/]  [grey](.env was generated for you)[/]");
        }
        else
        {
            AnsiConsole.MarkupLine($"  [grey]dotnet run --project[/] src/{Markup.Escape(options.MainProject)}");
        }
    }

    public static void RenderFailure(GenerationResult result)
    {
        var panel = new Panel(new Markup($"[red]{Markup.Escape(result.Error ?? "Generation failed.")}[/]"))
            .Header("[red]Generation failed[/]")
            .Border(BoxBorder.Rounded)
            .BorderColor(Color.Red);

        AnsiConsole.Write(panel);
    }

    public static void RenderHelp()
    {
        AnsiConsole.MarkupLine("[bold]monorepo-dotnet[/] — interactive ASP.NET Core solution generator");
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[grey]Usage:[/] monorepo [bold][[--help]][/]");
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("The CLI is interactive: it asks for the solution name, output directory,");
        AnsiConsole.MarkupLine("target framework, architecture (DDD, Clean, 1-Layer, MVC), database, Docker and optional packages.");
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[grey]Requirements:[/] .NET SDK 10.0.100+, Docker (optional), dotnet-ef (optional).");
    }

    private static string FormatPackages(OptionalPackage packages)
    {
        if (packages == OptionalPackage.None)
        {
            return "None";
        }

        return string.Join(", ", OptionalPackageExtensions.All()
            .Where(package => (packages & package) == package)
            .Select(package => package.ToString()));
    }
}
