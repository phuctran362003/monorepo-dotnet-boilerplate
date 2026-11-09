using System.Reflection;
using Microsoft.Extensions.Logging;
using MonorepoDotnet.CLI.Prompts;
using MonorepoDotnet.CLI.Rendering;
using MonorepoDotnet.Core.Abstractions;
using MonorepoDotnet.Core.Models;
using Spectre.Console;

namespace MonorepoDotnet.CLI;

/// <summary>Interactive workflow: prompt, validate, confirm, generate.</summary>
public sealed class CliApplication
{
    private readonly IEnvironmentValidator _environmentValidator;
    private readonly IDotNetSdkInstaller _dotNetSdkInstaller;
    private readonly ISolutionGenerator _solutionGenerator;
    private readonly IToolUpdateChecker _toolUpdateChecker;
    private readonly ConfigurationPrompt _configurationPrompt;
    private readonly ILogger<CliApplication> _logger;

    public CliApplication(
        IEnvironmentValidator environmentValidator,
        IDotNetSdkInstaller dotNetSdkInstaller,
        ISolutionGenerator solutionGenerator,
        IToolUpdateChecker toolUpdateChecker,
        ConfigurationPrompt configurationPrompt,
        ILogger<CliApplication> logger)
    {
        _environmentValidator = environmentValidator;
        _dotNetSdkInstaller = dotNetSdkInstaller;
        _solutionGenerator = solutionGenerator;
        _toolUpdateChecker = toolUpdateChecker;
        _configurationPrompt = configurationPrompt;
        _logger = logger;
    }

    public async Task<int> RunAsync(string[] args, CancellationToken cancellationToken)
    {
        if (args.Any(argument => argument is "--help" or "-h" or "/?"))
        {
            ConsoleRenderer.RenderHelp();
            return 0;
        }

        if (args.Any(argument => argument is "--version" or "-v"))
        {
            var version = CurrentVersion();
            AnsiConsole.MarkupLine($"monorepo-dotnet [green]{version}[/]");
            await NotifyIfUpdateAvailableAsync(version, cancellationToken);
            return 0;
        }

        if (Console.IsInputRedirected)
        {
            AnsiConsole.MarkupLine("[red]This tool needs an interactive terminal.[/]");
            AnsiConsole.MarkupLine(
                "[grey]Standard input is redirected, so the prompts cannot be answered. " +
                "Run monorepo from a console window.[/]");
            return 1;
        }

        ConsoleRenderer.RenderBanner();
        await NotifyIfUpdateAvailableAsync(CurrentVersion(), cancellationToken);

        var options = _configurationPrompt.Prompt();
        AnsiConsole.WriteLine();

        var validationRequest = new EnvironmentValidationRequest
        {
            RequireDocker = options.IncludeDocker,
            OutputDirectory = options.OutputDirectory,
            SolutionName = options.SolutionName,
            DotNetVersion = options.DotNetVersion
        };

        var report = await _environmentValidator.ValidateAsync(validationRequest, cancellationToken);
        ConsoleRenderer.RenderEnvironmentReport(report);

        if (report.HasErrors)
        {
            var sdkCheck = report.Errors.FirstOrDefault(check => check.Name == ".NET SDK");
            if (sdkCheck is not null)
            {
                ConsoleRenderer.RenderSdkInstallHelp(sdkCheck);

                if (AnsiConsole.Confirm("Install the .NET SDK from Microsoft now?", defaultValue: true))
                {
                    var installResult = await InstallSdkAsync(options.DotNetVersion, cancellationToken);
                    if (installResult.Succeeded)
                    {
                        AnsiConsole.MarkupLine("[green]SDK install finished. Re-checking the environment…[/]");
                        AnsiConsole.WriteLine();
                        report = await _environmentValidator.ValidateAsync(validationRequest, cancellationToken);
                        ConsoleRenderer.RenderEnvironmentReport(report);
                    }
                    else
                    {
                        AnsiConsole.MarkupLine("[red]SDK install did not complete.[/]");
                        var details = string.IsNullOrWhiteSpace(installResult.StandardError)
                            ? installResult.StandardOutput
                            : installResult.StandardError;
                        if (!string.IsNullOrWhiteSpace(details))
                        {
                            AnsiConsole.MarkupLine($"[grey]{Markup.Escape(details.Trim())}[/]");
                        }

                        AnsiConsole.MarkupLine("[grey]Run one of the Microsoft commands above in an elevated terminal, then retry.[/]");
                    }
                }
            }
        }

        if (report.HasErrors)
        {
            AnsiConsole.MarkupLine("[red]Environment validation failed. Fix the errors above and try again.[/]");
            return 1;
        }

        if (Directory.Exists(options.RootDirectory) &&
            Directory.EnumerateFileSystemEntries(options.RootDirectory).Any())
        {
            AnsiConsole.MarkupLine(
                $"[red]The target directory already exists and is not empty:[/] {Markup.Escape(options.RootDirectory)}");
            AnsiConsole.MarkupLine("[grey]Choose another solution name or output directory. Existing files are never overwritten.[/]");
            return 1;
        }

        ConsoleRenderer.RenderConfiguration(options);

        if (!AnsiConsole.Confirm("Generate this solution?", defaultValue: true))
        {
            AnsiConsole.MarkupLine("[yellow]Cancelled.[/]");
            return 1;
        }

        AnsiConsole.WriteLine();
        AnsiConsole.Write(new Rule("[bold cyan]Generating[/]").LeftJustified());
        AnsiConsole.WriteLine();

        GenerationResult? result = null;
        var spinner = new SpinnerColumn(Spinner.Known.Dots2)
        {
            Style = Color.Cyan1,
            CompletedText = "✓",
            CompletedStyle = Color.Green,
        };

        await AnsiConsole.Progress()
            .AutoClear(false)
            .HideCompleted(false)
            .Columns(
                spinner,
                new TaskDescriptionColumn(),
                new ProgressBarColumn
                {
                    Width = 28,
                    CompletedStyle = Color.Cyan1,
                    FinishedStyle = Color.Green,
                    RemainingStyle = Color.Grey,
                    IndeterminateStyle = new Style(foreground: Color.Cyan1, background: Color.Grey23),
                },
                new ElapsedTimeColumn { Style = Color.Grey })
            .StartAsync(async context =>
            {
                var task = context.AddTask("[grey]Starting…[/]", maxValue: 8);
                task.IsIndeterminate = true;

                var progress = new Progress<GenerationProgress>(update =>
                {
                    task.MaxValue = update.TotalSteps;
                    task.Description =
                        $"{Markup.Escape(update.Message)}…  [grey]{update.Step}/{update.TotalSteps}[/]";
                });

                result = await _solutionGenerator.GenerateAsync(options, progress, cancellationToken);
                task.IsIndeterminate = false;
                task.Value = task.MaxValue;

                if (result?.Success == true)
                {
                    task.Description = "[green]Generation complete[/]";
                    return;
                }

                spinner.CompletedText = "×";
                spinner.CompletedStyle = Color.Red;
                task.Description = "[red]Generation failed[/]";
            });

        AnsiConsole.WriteLine();

        if (result is null)
        {
            AnsiConsole.MarkupLine("[red]Generation produced no result.[/]");
            return 1;
        }

        if (!result.Success)
        {
            _logger.LogError("Generation failed: {Error}", result.Error);
            ConsoleRenderer.RenderFailure(result);
            return 1;
        }

        _logger.LogInformation("Solution generated at {RootDirectory}", result.RootDirectory);
        ConsoleRenderer.RenderSuccess(result, options);
        return 0;
    }

    private async Task<ProcessResult> InstallSdkAsync(string targetFramework, CancellationToken cancellationToken)
    {
        ProcessResult? installResult = null;

        await AnsiConsole.Status()
            .Spinner(Spinner.Known.Dots2)
            .StartAsync("Installing .NET SDK from Microsoft…", async _ =>
            {
                installResult = await _dotNetSdkInstaller.InstallAsync(targetFramework, cancellationToken);
            });

        return installResult ?? new ProcessResult(1, string.Empty, "SDK install produced no result.");
    }

    private async Task NotifyIfUpdateAvailableAsync(string currentVersion, CancellationToken cancellationToken)
    {
        var latest = await _toolUpdateChecker.FindNewerVersionAsync(currentVersion, cancellationToken);
        if (latest is null)
        {
            return;
        }

        ConsoleRenderer.RenderUpdateNotice(currentVersion, latest);
    }

    private static string CurrentVersion() =>
        Assembly.GetExecutingAssembly().GetName().Version?.ToString(3) ?? "0.0.0";
}
