using MonorepoDotnet.Core.Abstractions;
using MonorepoDotnet.Core.Generation;
using MonorepoDotnet.Core.Models;
using MonorepoDotnet.Core.Validation;

namespace MonorepoDotnet.Infrastructure.Environment;

/// <summary>Validates the .NET SDK, Docker, output directory and solution name.</summary>
public sealed class EnvironmentValidator : IEnvironmentValidator
{
    private readonly IProcessRunner _processRunner;
    private readonly IFileSystem _fileSystem;

    public EnvironmentValidator(IProcessRunner processRunner, IFileSystem fileSystem)
    {
        _processRunner = processRunner;
        _fileSystem = fileSystem;
    }

    public async Task<EnvironmentReport> ValidateAsync(
        EnvironmentValidationRequest request,
        CancellationToken cancellationToken = default)
    {
        var checks = new List<EnvironmentCheck>();

        await ValidateDotNetSdkAsync(checks, request.DotNetVersion, cancellationToken).ConfigureAwait(false);

        if (request.RequireDocker)
        {
            await ValidateDockerAsync(checks, cancellationToken).ConfigureAwait(false);
        }

        if (!string.IsNullOrWhiteSpace(request.OutputDirectory))
        {
            ValidateOutputDirectory(checks, request.OutputDirectory);
        }

        if (request.SolutionName is not null)
        {
            var errors = SolutionNameValidator.Validate(request.SolutionName);
            checks.Add(errors.Count == 0
                ? new EnvironmentCheck("Solution name", CheckSeverity.Info, "Solution name is valid.")
                : new EnvironmentCheck("Solution name", CheckSeverity.Error, string.Join(" ", errors)));
        }

        if (!string.IsNullOrWhiteSpace(request.OutputDirectory) && request.SolutionName is not null)
        {
            ValidateSolutionPath(checks, request.OutputDirectory, request.SolutionName);
        }

        return new EnvironmentReport(checks);
    }

    private async Task ValidateDotNetSdkAsync(
        List<EnvironmentCheck> checks,
        string? targetFramework,
        CancellationToken cancellationToken)
    {
        var profile = ResolveProfile(targetFramework);
        var installCommands = DotNetVersionCatalog.GetInstallCommands(profile);

        try
        {
            DotNetExecutable.PrependInstallLocationsToPath();

            var result = await _processRunner
                .RunAsync(DotNetExecutable.Resolve(), ["--list-sdks"], cancellationToken: cancellationToken)
                .ConfigureAwait(false);

            if (!result.Succeeded)
            {
                checks.Add(MissingSdkCheck(
                    profile,
                    $"Unable to list installed .NET SDKs. {result.StandardError.Trim()}".Trim(),
                    installCommands));
                return;
            }

            var installed = DotNetSdkVersionParser.ParseListSdks(result.StandardOutput);
            if (!DotNetSdkVersionParser.HasCompatibleSdk(installed, profile.Major))
            {
                var found = installed.Count == 0
                    ? "No .NET SDK is installed."
                    : $"Found {string.Join(", ", installed.Select(sdk => sdk.ToString()))}.";

                checks.Add(MissingSdkCheck(
                    profile,
                    $"{profile.DisplayName} SDK ({profile.TargetFramework}) is required to generate and build this solution. {found}",
                    installCommands));
                return;
            }

            var best = installed
                .Where(sdk => sdk.Major >= profile.Major)
                .OrderByDescending(sdk => sdk)
                .First();

            checks.Add(new EnvironmentCheck(
                ".NET SDK",
                CheckSeverity.Info,
                $"Found .NET SDK {best} (compatible with {profile.TargetFramework})."));
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            checks.Add(MissingSdkCheck(
                profile,
                $".NET SDK is not available: {ex.Message}",
                installCommands));
        }
    }

    private static DotNetVersionProfile ResolveProfile(string? targetFramework) =>
        !string.IsNullOrWhiteSpace(targetFramework) && DotNetVersionCatalog.IsSupported(targetFramework)
            ? DotNetVersionCatalog.Get(targetFramework)
            : DotNetVersionCatalog.Net10;

    private static EnvironmentCheck MissingSdkCheck(
        DotNetVersionProfile profile,
        string message,
        IReadOnlyList<string> installCommands) =>
        new(
            ".NET SDK",
            CheckSeverity.Error,
            message,
            installCommands,
            profile.DownloadUrl);

    private async Task ValidateDockerAsync(
        List<EnvironmentCheck> checks,
        CancellationToken cancellationToken)
    {
        try
        {
            var cliResult = await _processRunner
                .RunAsync("docker", ["--version"], cancellationToken: cancellationToken)
                .ConfigureAwait(false);

            if (!cliResult.Succeeded)
            {
                checks.Add(new EnvironmentCheck(
                    "Docker CLI",
                    CheckSeverity.Warning,
                    "Docker CLI was not found. Docker files will still be generated, but containers cannot be started."));
                return;
            }

            checks.Add(new EnvironmentCheck(
                "Docker CLI",
                CheckSeverity.Info,
                cliResult.StandardOutput.Trim()));

            var engineResult = await _processRunner
                .RunAsync("docker", ["info", "--format", "{{.ServerVersion}}"], cancellationToken: cancellationToken)
                .ConfigureAwait(false);

            checks.Add(engineResult.Succeeded
                ? new EnvironmentCheck("Docker Engine", CheckSeverity.Info, $"Docker Engine {engineResult.StandardOutput.Trim()} is running.")
                : new EnvironmentCheck(
                    "Docker Engine",
                    CheckSeverity.Warning,
                    "Docker Engine is not running. Docker files will still be generated; start Docker Desktop before running containers."));
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            checks.Add(new EnvironmentCheck(
                "Docker",
                CheckSeverity.Warning,
                $"Docker validation failed: {ex.Message}. Docker files will still be generated."));
        }
    }

    private void ValidateOutputDirectory(List<EnvironmentCheck> checks, string outputDirectory)
    {
        var pathErrors = OutputPathValidator.Validate(outputDirectory);
        if (pathErrors.Count > 0)
        {
            checks.Add(new EnvironmentCheck(
                "Output directory",
                CheckSeverity.Error,
                string.Join(" ", pathErrors)));
            return;
        }

        var fullPath = OutputPathValidator.Normalize(outputDirectory);
        var probeFile = Path.Combine(fullPath, $".monorepo-dotnet-write-test-{Guid.NewGuid():N}");

        try
        {
            _fileSystem.CreateDirectory(fullPath);
            _fileSystem.WriteAllTextAsync(probeFile, "probe").GetAwaiter().GetResult();
            _fileSystem.DeleteFile(probeFile);

            checks.Add(new EnvironmentCheck(
                "Output directory",
                CheckSeverity.Info,
                $"Output directory is writable: {fullPath}"));
        }
        catch (Exception ex)
        {
            checks.Add(new EnvironmentCheck(
                "Output directory",
                CheckSeverity.Error,
                $"Output directory '{fullPath}' is not writable: {ex.Message}"));
        }
        finally
        {
            try
            {
                _fileSystem.DeleteFile(probeFile);
            }
            catch
            {
                // Best effort cleanup.
            }
        }
    }

    private static void ValidateSolutionPath(
        List<EnvironmentCheck> checks,
        string outputDirectory,
        string solutionName)
    {
        // The dedicated output-directory and solution-name checks already report
        // these problems; only probe the combined path when both inputs are valid.
        if (OutputPathValidator.Validate(outputDirectory).Count > 0 ||
            !SolutionNameValidator.IsValid(solutionName))
        {
            return;
        }

        var errors = OutputPathValidator.ValidateTarget(outputDirectory, solutionName);

        if (errors.Count > 0)
        {
            checks.Add(new EnvironmentCheck(
                "Solution path",
                CheckSeverity.Error,
                string.Join(" ", errors)));
            return;
        }

        var targetPath = Path.Combine(OutputPathValidator.Normalize(outputDirectory), solutionName.Trim());
        checks.Add(new EnvironmentCheck(
            "Solution path",
            CheckSeverity.Info,
            $"Solution path is available: {targetPath}"));
    }
}
