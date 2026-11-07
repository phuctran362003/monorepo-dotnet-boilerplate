using MonorepoDotnet.Core.Abstractions;
using MonorepoDotnet.Core.Models;
using MonorepoDotnet.Core.Validation;

namespace MonorepoDotnet.Infrastructure.Environment;

/// <summary>Validates the .NET SDK, Docker, output directory and solution name.</summary>
public sealed class EnvironmentValidator : IEnvironmentValidator
{
    private const int MinimumDotNetMajorVersion = 10;
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

        await ValidateDotNetSdkAsync(checks, cancellationToken).ConfigureAwait(false);

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

        return new EnvironmentReport(checks);
    }

    private async Task ValidateDotNetSdkAsync(
        List<EnvironmentCheck> checks,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _processRunner
                .RunAsync("dotnet", ["--version"], cancellationToken: cancellationToken)
                .ConfigureAwait(false);

            if (!result.Succeeded)
            {
                checks.Add(new EnvironmentCheck(
                    ".NET SDK",
                    CheckSeverity.Error,
                    $"Unable to read the .NET SDK version. {result.StandardError.Trim()}"));
                return;
            }

            var rawVersion = result.StandardOutput.Trim().Split('\n')[0].Trim();
            var numeric = rawVersion.Split('-')[0];
            var major = Version.TryParse(numeric, out var version) ? version.Major : 0;

            if (major < MinimumDotNetMajorVersion)
            {
                checks.Add(new EnvironmentCheck(
                    ".NET SDK",
                    CheckSeverity.Error,
                    $".NET SDK {MinimumDotNetMajorVersion}.0 or later is required. Found {rawVersion}."));
                return;
            }

            checks.Add(new EnvironmentCheck(
                ".NET SDK",
                CheckSeverity.Info,
                $"Found .NET SDK {rawVersion}."));
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            checks.Add(new EnvironmentCheck(
                ".NET SDK",
                CheckSeverity.Error,
                $".NET SDK is not available: {ex.Message}"));
        }
    }

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
        var probeFile = Path.Combine(
            Path.GetFullPath(outputDirectory),
            $".monorepo-dotnet-write-test-{Guid.NewGuid():N}");

        try
        {
            _fileSystem.CreateDirectory(outputDirectory);
            _fileSystem.WriteAllTextAsync(probeFile, "probe").GetAwaiter().GetResult();
            _fileSystem.DeleteFile(probeFile);

            checks.Add(new EnvironmentCheck(
                "Output directory",
                CheckSeverity.Info,
                $"Output directory is writable: {Path.GetFullPath(outputDirectory)}"));
        }
        catch (Exception ex)
        {
            checks.Add(new EnvironmentCheck(
                "Output directory",
                CheckSeverity.Error,
                $"Output directory '{outputDirectory}' is not writable: {ex.Message}"));
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
}
