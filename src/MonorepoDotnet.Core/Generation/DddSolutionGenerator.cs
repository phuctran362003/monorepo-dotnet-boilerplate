using System.Security.Cryptography;
using MonorepoDotnet.Core.Abstractions;
using MonorepoDotnet.Core.Models;
using MonorepoDotnet.Core.Validation;

namespace MonorepoDotnet.Core.Generation;

/// <summary>
/// Orchestrates the DDD solution generation pipeline. The generator only talks
/// to abstractions, so it stays testable and free of infrastructure concerns.
/// </summary>
public sealed class DddSolutionGenerator : ISolutionGenerator
{
    private const int TotalSteps = 8;
    private readonly IFileSystem _fileSystem;
    private readonly ITemplateProvider _templateProvider;
    private readonly ITemplateRenderer _templateRenderer;
    private readonly IProcessRunner _processRunner;

    public DddSolutionGenerator(
        IFileSystem fileSystem,
        ITemplateProvider templateProvider,
        ITemplateRenderer templateRenderer,
        IProcessRunner processRunner)
    {
        _fileSystem = fileSystem;
        _templateProvider = templateProvider;
        _templateRenderer = templateRenderer;
        _processRunner = processRunner;
    }

    public async Task<GenerationResult> GenerateAsync(
        GeneratorOptions options,
        IProgress<GenerationProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(options);

        var warnings = new List<string>();
        var rootDirectory = options.RootDirectory;
        var createdRoot = false;

        try
        {
            ValidateOptions(options);

            createdRoot = PrepareTargetDirectory(rootDirectory);
            var files = DddTemplateManifest.ForOptions(options).ToList();

            progress?.Report(new GenerationProgress(1, TotalSteps, "Preparing solution directory"));

            var values = BuildTemplateValues(options);
            var flags = BuildTemplateFlags(options);

            progress?.Report(new GenerationProgress(2, TotalSteps, $"Generating {files.Count} project files"));
            await WriteTemplateFilesAsync(rootDirectory, files, values, flags, cancellationToken)
                .ConfigureAwait(false);

            if (options.IncludeDocker)
            {
                progress?.Report(new GenerationProgress(3, TotalSteps, "Writing Docker environment"));
                await WriteEnvironmentFileAsync(rootDirectory, options, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                progress?.Report(new GenerationProgress(3, TotalSteps, "Skipping Docker environment"));
            }

            progress?.Report(new GenerationProgress(4, TotalSteps, "Creating solution file"));
            await CreateSolutionAsync(rootDirectory, options, cancellationToken).ConfigureAwait(false);

            progress?.Report(new GenerationProgress(5, TotalSteps, "Restoring NuGet packages"));
            await RunDotnetAsync(rootDirectory, ["restore", SolutionFileName(options)], cancellationToken)
                .ConfigureAwait(false);

            progress?.Report(new GenerationProgress(6, TotalSteps, "Building generated solution"));
            await RunDotnetAsync(
                    rootDirectory,
                    ["build", SolutionFileName(options), "--no-restore", "-c", "Debug", "--nologo"],
                    cancellationToken)
                .ConfigureAwait(false);

            progress?.Report(new GenerationProgress(7, TotalSteps, "Creating initial EF Core migration"));
            var migrationCreated = await TryCreateInitialMigrationAsync(rootDirectory, options, warnings, cancellationToken)
                .ConfigureAwait(false);

            progress?.Report(new GenerationProgress(8, TotalSteps, "Finalizing"));

            if (!migrationCreated)
            {
                warnings.Add(
                    "The initial EF Core migration was not generated. Install the EF tool " +
                    "('dotnet tool install --global dotnet-ef --version 10.0.14') and run the migration command from the generated README.");
            }

            return GenerationResult.Ok(rootDirectory, warnings);
        }
        catch (OperationCanceledException)
        {
            CleanupOnFailure(rootDirectory, createdRoot);
            throw;
        }
        catch (Exception ex) when (ex is GenerationException or InvalidOperationException)
        {
            CleanupOnFailure(rootDirectory, createdRoot);
            return GenerationResult.Failed(rootDirectory, ex.Message);
        }
    }

    private static void ValidateOptions(GeneratorOptions options)
    {
        var nameErrors = SolutionNameValidator.Validate(options.SolutionName);
        if (nameErrors.Count > 0)
        {
            throw new GenerationException(string.Join(" ", nameErrors));
        }

        if (string.IsNullOrWhiteSpace(options.OutputDirectory))
        {
            throw new GenerationException("Output directory is required.");
        }

        if (options.Architecture != ArchitectureType.Ddd)
        {
            throw new GenerationException($"Architecture '{options.Architecture}' is not implemented yet.");
        }

        if (options.Database != DatabaseProvider.PostgreSql)
        {
            throw new GenerationException($"Database '{options.Database}' is not implemented yet.");
        }

        if (options.DotNetVersion != GeneratorOptions.DefaultDotNetVersion)
        {
            throw new GenerationException($"Target framework '{options.DotNetVersion}' is not implemented yet.");
        }
    }

    private bool PrepareTargetDirectory(string rootDirectory)
    {
        var existed = _fileSystem.DirectoryExists(rootDirectory);

        if (existed && !_fileSystem.IsDirectoryEmpty(rootDirectory))
        {
            throw new GenerationException(
                $"The output directory '{rootDirectory}' already exists and is not empty. " +
                "Choose a different solution name or output directory. Existing files are never overwritten.");
        }

        _fileSystem.CreateDirectory(rootDirectory);
        return !existed;
    }

    private async Task WriteTemplateFilesAsync(
        string rootDirectory,
        IReadOnlyList<TemplateFile> files,
        IReadOnlyDictionary<string, string> values,
        IReadOnlyDictionary<string, bool> flags,
        CancellationToken cancellationToken)
    {
        foreach (var file in files)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var template = await _templateProvider.ReadTemplateAsync(file.TemplatePath, cancellationToken)
                .ConfigureAwait(false);
            var content = _templateRenderer.Render(template, values, flags);
            var outputPath = _templateRenderer.Render(file.OutputPath, values, flags);
            var absolutePath = Path.Combine(rootDirectory, outputPath.Replace('/', Path.DirectorySeparatorChar));

            await _fileSystem.WriteAllTextAsync(absolutePath, content, cancellationToken).ConfigureAwait(false);
        }
    }

    private async Task WriteEnvironmentFileAsync(
        string rootDirectory,
        GeneratorOptions options,
        CancellationToken cancellationToken)
    {
        var password = Convert.ToHexString(RandomNumberGenerator.GetBytes(24)).ToLowerInvariant();
        var content =
            $"# Local Docker Compose environment. Never commit this file.{Environment.NewLine}" +
            $"POSTGRES_DB={options.DockerName}{Environment.NewLine}" +
            "POSTGRES_USER=postgres" + Environment.NewLine +
            $"POSTGRES_PASSWORD={password}{Environment.NewLine}" +
            "POSTGRES_PORT=5432" + Environment.NewLine +
            "API_HTTP_PORT=8080" + Environment.NewLine +
            "APPLY_MIGRATIONS=false" + Environment.NewLine;

        await _fileSystem
            .WriteAllTextAsync(Path.Combine(rootDirectory, ".env"), content, cancellationToken)
            .ConfigureAwait(false);
    }

    private async Task CreateSolutionAsync(
        string rootDirectory,
        GeneratorOptions options,
        CancellationToken cancellationToken)
    {
        var solutionFile = SolutionFileName(options);

        if (_fileSystem.FileExists(Path.Combine(rootDirectory, solutionFile)))
        {
            throw new GenerationException($"Solution file '{solutionFile}' already exists.");
        }

        var newSolution = await _processRunner
            .RunAsync(
                "dotnet",
                ["new", "sln", "--name", options.SolutionName, "--output", rootDirectory, "--format", "sln"],
                rootDirectory,
                cancellationToken)
            .ConfigureAwait(false);

        if (!newSolution.Succeeded)
        {
            throw new GenerationException(
                $"Could not create the solution file. {newSolution.StandardError.Trim()}");
        }

        string[] projects =
        [
            ProjectPath(options.DomainProject),
            ProjectPath(options.ApplicationProject),
            ProjectPath(options.InfrastructureProject),
            ProjectPath(options.ApiProject)
        ];

        var addProjects = await _processRunner
            .RunAsync(
                "dotnet",
                ["sln", solutionFile, "add", .. projects],
                rootDirectory,
                cancellationToken)
            .ConfigureAwait(false);

        if (!addProjects.Succeeded)
        {
            throw new GenerationException(
                $"Could not add projects to the solution. {addProjects.StandardError.Trim()}");
        }
    }

    private async Task RunDotnetAsync(
        string rootDirectory,
        string[] arguments,
        CancellationToken cancellationToken)
    {
        var result = await _processRunner
            .RunAsync("dotnet", arguments, rootDirectory, cancellationToken)
            .ConfigureAwait(false);

        if (!result.Succeeded)
        {
            var detail = string.IsNullOrWhiteSpace(result.StandardError)
                ? result.StandardOutput
                : result.StandardError;

            throw new GenerationException(
                $"Command 'dotnet {string.Join(' ', arguments)}' failed with exit code {result.ExitCode}.{Environment.NewLine}" +
                TrimForDisplay(detail));
        }
    }

    private async Task<bool> TryCreateInitialMigrationAsync(
        string rootDirectory,
        GeneratorOptions options,
        List<string> warnings,
        CancellationToken cancellationToken)
    {
        var toolCheck = await _processRunner
            .RunAsync("dotnet", ["ef", "--version"], rootDirectory, cancellationToken)
            .ConfigureAwait(false);

        if (!toolCheck.Succeeded)
        {
            return false;
        }

        var result = await _processRunner
            .RunAsync(
                "dotnet",
                [
                    "ef", "migrations", "add", "InitialCreate",
                    "--project", ProjectPath(options.InfrastructureProject),
                    "--startup-project", ProjectPath(options.ApiProject),
                    "--output-dir", "Persistence/Migrations",
                    "--no-build"
                ],
                rootDirectory,
                cancellationToken)
            .ConfigureAwait(false);

        if (!result.Succeeded)
        {
            warnings.Add(
                "The initial EF Core migration could not be created automatically. " +
                TrimForDisplay(result.StandardError));
            return false;
        }

        return true;
    }

    private void CleanupOnFailure(string rootDirectory, bool createdRoot)
    {
        if (!createdRoot)
        {
            return;
        }

        try
        {
            _fileSystem.DeleteDirectory(rootDirectory, recursive: true);
        }
        catch
        {
            // Best effort: never mask the original failure.
        }
    }

    private static string ProjectPath(string projectName) =>
        $"src/{projectName}/{projectName}.csproj";

    private static string SolutionFileName(GeneratorOptions options) => $"{options.SolutionName}.sln";

    private static string TrimForDisplay(string value)
    {
        const int maxLength = 4000;
        var trimmed = value.Trim();
        return trimmed.Length <= maxLength ? trimmed : trimmed[..maxLength] + "...";
    }

    private static IReadOnlyDictionary<string, string> BuildTemplateValues(GeneratorOptions options) =>
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["SolutionName"] = options.SolutionName,
            ["Namespace"] = options.SolutionName,
            ["DomainProject"] = options.DomainProject,
            ["ApplicationProject"] = options.ApplicationProject,
            ["InfrastructureProject"] = options.InfrastructureProject,
            ["ApiProject"] = options.ApiProject,
            ["TargetFramework"] = options.DotNetVersion,
            ["DotNetImageTag"] = PackageVersions.DotNetImageTag,
            ["DockerName"] = options.DockerName,
            ["DatabaseName"] = options.DockerName,
            ["PostgresUser"] = "postgres",
            ["PostgresPort"] = "5432",
            ["ApiHttpPort"] = "8080",
            ["ApiHttpsPort"] = "8081",
            ["EfCoreVersion"] = PackageVersions.EntityFrameworkCore,
            ["NpgsqlVersion"] = PackageVersions.NpgsqlEntityFrameworkCore,
            ["OpenApiVersion"] = PackageVersions.AspNetCoreOpenApi,
            ["ExtensionsVersion"] = PackageVersions.MicrosoftExtensions,
            ["SerilogVersion"] = PackageVersions.SerilogAspNetCore,
            ["FluentValidationVersion"] = PackageVersions.FluentValidation,
            ["AutoMapperVersion"] = PackageVersions.AutoMapper,
            ["MediatRVersion"] = PackageVersions.MediatR,
            ["RedisVersion"] = PackageVersions.StackExchangeRedis,
            ["PostgresImage"] = PackageVersions.PostgresImage,
            ["RedisImage"] = PackageVersions.RedisImage
        };

    private static IReadOnlyDictionary<string, bool> BuildTemplateFlags(GeneratorOptions options) =>
        new Dictionary<string, bool>(StringComparer.Ordinal)
        {
            ["Docker"] = options.IncludeDocker,
            ["Serilog"] = options.HasPackage(OptionalPackage.Serilog),
            ["FluentValidation"] = options.HasPackage(OptionalPackage.FluentValidation),
            ["AutoMapper"] = options.HasPackage(OptionalPackage.AutoMapper),
            ["Redis"] = options.HasPackage(OptionalPackage.Redis),
            ["MediatR"] = options.HasPackage(OptionalPackage.MediatR)
        };
}
