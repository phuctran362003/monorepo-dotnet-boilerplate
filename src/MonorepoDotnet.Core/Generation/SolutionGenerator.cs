using System.Security.Cryptography;
using MonorepoDotnet.Core.Abstractions;
using MonorepoDotnet.Core.Models;
using MonorepoDotnet.Core.Validation;

namespace MonorepoDotnet.Core.Generation;

/// <summary>
/// Orchestrates the solution generation pipeline for every supported
/// architecture. The generator only talks to abstractions, so it stays
/// testable and free of infrastructure concerns.
/// </summary>
public sealed class SolutionGenerator : ISolutionGenerator
{
    private const int TotalSteps = 7;
    private readonly IFileSystem _fileSystem;
    private readonly ITemplateProvider _templateProvider;
    private readonly ITemplateRenderer _templateRenderer;
    private readonly IProcessRunner _processRunner;

    public SolutionGenerator(
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

            var layout = SolutionLayouts.Create(options);
            createdRoot = PrepareTargetDirectory(rootDirectory);
            var files = ArchitectureTemplateManifest.For(options).ToList();

            progress?.Report(new GenerationProgress(1, TotalSteps, "Preparing solution directory"));

            var values = BuildTemplateValues(options, layout);
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
            await CreateSolutionAsync(rootDirectory, options, layout, cancellationToken).ConfigureAwait(false);

            progress?.Report(new GenerationProgress(5, TotalSteps, "Restoring NuGet packages"));
            await RestoreWithFallbackAsync(rootDirectory, SolutionFileName(options), warnings, cancellationToken)
                .ConfigureAwait(false);

            progress?.Report(new GenerationProgress(6, TotalSteps, "Building generated solution"));
            await RunDotnetAsync(
                    rootDirectory,
                    ["build", SolutionFileName(options), "--no-restore", "-c", "Debug", "--nologo"],
                    cancellationToken)
                .ConfigureAwait(false);

            progress?.Report(new GenerationProgress(7, TotalSteps, "Finalizing"));

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

        var pathErrors = OutputPathValidator.ValidateTarget(options.OutputDirectory, options.SolutionName);
        if (pathErrors.Count > 0)
        {
            throw new GenerationException(string.Join(" ", pathErrors));
        }

        if (!Enum.IsDefined(options.ProjectKind) || !Enum.IsDefined(options.Architecture) ||
            !options.ProjectKind.IsImplemented(options.Architecture))
        {
            throw new GenerationException(
                $"Application '{options.ProjectKind.ToDisplayName()}' with architecture '{options.Architecture}' is not implemented yet.");
        }

        if (options.Database != DatabaseProvider.PostgreSql &&
            options.Database != DatabaseProvider.SqlServer)
        {
            throw new GenerationException($"Database '{options.Database}' is not implemented yet.");
        }

        if (!DotNetVersionCatalog.IsSupported(options.DotNetVersion))
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
        var newLine = Environment.NewLine;

        var databaseVariables = options.Database switch
        {
            // SQL Server requires at least 8 characters with three of the four
            // categories (upper, lower, digit, symbol); a hex password plus two
            // extra symbols satisfies that.
            DatabaseProvider.SqlServer =>
                $"MSSQL_DB={options.DockerName}{newLine}" +
                "MSSQL_USER=sa" + newLine +
                $"MSSQL_SA_PASSWORD={password}Aa!{newLine}" +
                $"MSSQL_PORT={options.DatabasePort}{newLine}",
            _ =>
                $"POSTGRES_DB={options.DockerName}{newLine}" +
                "POSTGRES_USER=postgres" + newLine +
                $"POSTGRES_PASSWORD={password}{newLine}" +
                $"POSTGRES_PORT={options.DatabasePort}{newLine}"
        };

        var httpPort = options.ProjectKind == ProjectKind.Console
            ? string.Empty
            : "API_HTTP_PORT=8080" + newLine;

        var content =
            $"# Local Docker Compose environment. Never commit this file.{newLine}" +
            databaseVariables +
            httpPort +
            "APPLY_MIGRATIONS=false" + newLine;

        await _fileSystem
            .WriteAllTextAsync(Path.Combine(rootDirectory, ".env"), content, cancellationToken)
            .ConfigureAwait(false);
    }

    private async Task CreateSolutionAsync(
        string rootDirectory,
        GeneratorOptions options,
        SolutionLayout layout,
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
                ["new", "sln", "--name", options.SolutionName, "--output", rootDirectory, "--format", SolutionFormat(options)],
                rootDirectory,
                cancellationToken)
            .ConfigureAwait(false);

        if (!newSolution.Succeeded)
        {
            throw new GenerationException(
                $"Could not create the solution file. {newSolution.StandardError.Trim()}");
        }

        var addProjects = await _processRunner
            .RunAsync(
                "dotnet",
                ["sln", solutionFile, "add", .. layout.Projects.Select(project => project.RelativePath)],
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

    /// <summary>
    /// Restores the solution. If NuGet reports a package downgrade (NU1605) as an
    /// error, the restore is retried with that check demoted to a warning so the
    /// solution is still generated. The downgrade is surfaced as a warning in the
    /// generation result so the template versions can be aligned later.
    /// </summary>
    private async Task RestoreWithFallbackAsync(
        string rootDirectory,
        string solutionFile,
        List<string> warnings,
        CancellationToken cancellationToken)
    {
        var arguments = new[] { "restore", solutionFile };
        var result = await _processRunner
            .RunAsync("dotnet", arguments, rootDirectory, cancellationToken)
            .ConfigureAwait(false);

        if (result.Succeeded)
        {
            return;
        }

        if (!HasDowngradeError(result))
        {
            throw CommandFailure(arguments, result);
        }

        var fallbackArguments = new[] { "restore", solutionFile, "-p:WarningsAsErrors=" };
        var fallback = await _processRunner
            .RunAsync("dotnet", fallbackArguments, rootDirectory, cancellationToken)
            .ConfigureAwait(false);

        if (!fallback.Succeeded)
        {
            throw CommandFailure(fallbackArguments, fallback);
        }

        warnings.Add(
            "NuGet detected a package downgrade (NU1605). The solution was generated with NuGet's resolved " +
            "versions; align the package versions in DotNetVersionCatalog to remove this warning.");
    }

    private static bool HasDowngradeError(ProcessResult result) =>
        result.StandardOutput.Contains("NU1605", StringComparison.Ordinal) ||
        result.StandardError.Contains("NU1605", StringComparison.Ordinal);

    private static GenerationException CommandFailure(string[] arguments, ProcessResult result)
    {
        var detail = string.IsNullOrWhiteSpace(result.StandardError)
            ? result.StandardOutput
            : result.StandardError;

        return new GenerationException(
            $"Command 'dotnet {string.Join(' ', arguments)}' failed with exit code {result.ExitCode}.{Environment.NewLine}" +
            TrimForDisplay(detail));
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

    private static string SolutionFileName(GeneratorOptions options) =>
        $"{options.SolutionName}.{(options.Architecture == ArchitectureType.Standard ? "slnx" : "sln")}";

    private static string SolutionFormat(GeneratorOptions options) =>
        options.Architecture == ArchitectureType.Standard ? "slnx" : "sln";

    private static string TrimForDisplay(string value)
    {
        const int maxLength = 4000;
        var trimmed = value.Trim();
        return trimmed.Length <= maxLength ? trimmed : trimmed[..maxLength] + "...";
    }

    private static IReadOnlyDictionary<string, string> BuildTemplateValues(
        GeneratorOptions options,
        SolutionLayout layout)
    {
        var packages = PackageVersions.For(options.DotNetVersion);

        return new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["SolutionName"] = options.SolutionName,
            ["BaseEntityNamespace"] = options.Architecture is ArchitectureType.Ddd or ArchitectureType.Clean
                ? $"{options.DomainProject}.Entities"
                : $"{options.MainProject}.Entities",
            ["RepositoryInterfaceNamespace"] = options.Architecture is ArchitectureType.Ddd or ArchitectureType.Clean
                ? $"{options.DomainProject}.Repositories"
                : $"{options.MainProject}.Data",
            ["RepositoryImplementationNamespace"] = options.Architecture is ArchitectureType.Ddd or ArchitectureType.Clean
                ? $"{options.InfrastructureProject}.Persistence.Repositories"
                : $"{options.MainProject}.Data.Repositories",
            ["UnitOfWorkInterfaceNamespace"] = options.Architecture is ArchitectureType.Ddd or ArchitectureType.Clean
                ? $"{options.ApplicationProject}.Common.Interfaces"
                : $"{options.MainProject}.Common",
            ["UnitOfWorkImplementationNamespace"] = options.Architecture is ArchitectureType.Ddd or ArchitectureType.Clean
                ? $"{options.InfrastructureProject}.Persistence"
                : $"{options.MainProject}.Data",
            ["DbContextNamespace"] = options.Architecture is ArchitectureType.Ddd or ArchitectureType.Clean
                ? $"{options.InfrastructureProject}.Persistence"
                : $"{options.MainProject}.Data",
            ["ProductEntityNamespace"] = options.Architecture is ArchitectureType.Ddd or ArchitectureType.Clean
                ? $"{options.DomainProject}.Entities"
                : $"{options.MainProject}.Models",
            ["Namespace"] = layout.MainProject.Name,
            ["DomainProject"] = options.DomainProject,
            ["ApplicationProject"] = options.ApplicationProject,
            ["InfrastructureProject"] = options.InfrastructureProject,
            ["ApiProject"] = options.MainProject,
            ["ServiceProject"] = options.ServiceProject,
            ["ConsoleProject"] = options.ConsoleProject,
            ["ComposeService"] = options.ProjectKind == ProjectKind.Console ? "app" : "api",
            ["WebApiProject"] = options.WebApiProject,
            ["SingleLayerProject"] = options.SingleLayerProject,
            ["MvcProject"] = options.MvcProject,
            ["DomainNamespace"] = options.DomainProject,
            ["ApplicationNamespace"] = options.ApplicationProject,
            ["InfrastructureNamespace"] = options.InfrastructureProject,
            ["ApiNamespace"] = options.MainProject,
            ["MainProject"] = layout.MainProject.Name,
            ["MainProjectPath"] = layout.MainProject.RelativePath,
            ["DockerProjectCopies"] = BuildDockerProjectCopies(layout),
            ["TargetFramework"] = options.DotNetVersion,
            ["DotNetImageTag"] = packages.ImageTag,
            ["DockerName"] = options.DockerName,
            ["DatabaseName"] = options.DockerName,
            ["Database"] = options.Database.ToShortName(),
            ["DatabaseService"] = options.DatabaseService,
            ["DatabaseUser"] = options.DatabaseUser,
            ["DatabasePort"] = options.DatabasePort.ToString(),
            ["ApiHttpPort"] = "8080",
            ["ApiHttpsPort"] = "8081",
            ["EfCoreVersion"] = packages.EntityFrameworkCore,
            ["NpgsqlVersion"] = packages.NpgsqlEntityFrameworkCore,
            ["OpenApiVersion"] = packages.AspNetCoreOpenApi,
            ["SwaggerUiVersion"] = PackageVersions.SwashbuckleSwaggerUi,
            ["ExtensionsVersion"] = packages.MicrosoftExtensions,
            ["ExtensionsAbstractionsVersion"] = packages.ExtensionsAbstractions,
            ["JwtBearerVersion"] = packages.JwtBearer,
            ["ODataVersion"] = packages.AspNetCoreOData,
            ["FluentValidationAspNetCoreVersion"] = packages.FluentValidationAspNetCore,
            ["CachingRedisVersion"] = packages.CachingRedis,
            ["SerilogVersion"] = packages.SerilogAspNetCore,
            ["FluentValidationVersion"] = PackageVersions.FluentValidation,
            ["AutoMapperVersion"] = PackageVersions.AutoMapper,
            ["MediatRVersion"] = PackageVersions.MediatR,
            ["RedisVersion"] = PackageVersions.StackExchangeRedis,
            ["PostgresImage"] = PackageVersions.PostgresImage,
            ["SqlServerImage"] = PackageVersions.SqlServerImage,
            ["RedisImage"] = PackageVersions.RedisImage
        };
    }

    private static string BuildDockerProjectCopies(SolutionLayout layout) =>
        string.Join(
            Environment.NewLine,
            layout.Projects.Select(project =>
                $"COPY [\"{project.RelativePath}\", \"src/{project.Name}/\"]"));

    private static IReadOnlyDictionary<string, bool> BuildTemplateFlags(GeneratorOptions options) =>
        new Dictionary<string, bool>(StringComparer.Ordinal)
        {
            ["Docker"] = options.IncludeDocker,
            ["Serilog"] = options.HasPackage(OptionalPackage.Serilog),
            ["FluentValidation"] = options.HasPackage(OptionalPackage.FluentValidation),
            ["AutoMapper"] = options.HasPackage(OptionalPackage.AutoMapper),
            ["Redis"] = options.HasPackage(OptionalPackage.Redis),
            ["MediatR"] = options.HasPackage(OptionalPackage.MediatR),
            ["Standard"] = options.Architecture == ArchitectureType.Standard,
            ["Ddd"] = options.Architecture == ArchitectureType.Ddd,
            ["Clean"] = options.Architecture == ArchitectureType.Clean,
            ["SingleLayer"] = options.Architecture == ArchitectureType.SingleLayer,
            ["Mvc"] = options.Architecture == ArchitectureType.Mvc,
            ["Console"] = options.ProjectKind == ProjectKind.Console,
            ["Postgres"] = options.Database == DatabaseProvider.PostgreSql,
            ["SqlServer"] = options.Database == DatabaseProvider.SqlServer
        };
}
