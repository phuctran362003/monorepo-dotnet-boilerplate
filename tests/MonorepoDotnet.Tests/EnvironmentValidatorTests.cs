using MonorepoDotnet.Core.Abstractions;
using MonorepoDotnet.Core.Generation;
using MonorepoDotnet.Core.Models;
using MonorepoDotnet.Core.Validation;
using MonorepoDotnet.Infrastructure.Environment;

namespace MonorepoDotnet.Tests;

public class EnvironmentValidatorTests
{
    private readonly FakeProcessRunner _runner = new();
    private readonly FakeFileSystem _fileSystem = new();
    private readonly EnvironmentValidator _validator;

    public EnvironmentValidatorTests()
    {
        _validator = new EnvironmentValidator(_runner, _fileSystem);
    }

    [Fact]
    public async Task Validate_ReturnsNetSdkInfo_WhenCompatibleSdkInstalled()
    {
        _runner.SetResult("dotnet", new ProcessResult(0, "10.0.100 [C:\\Program Files\\dotnet\\sdk]", string.Empty));

        var report = await _validator.ValidateAsync(new EnvironmentValidationRequest());

        var sdkCheck = Assert.Single(report.Checks, c => c.Name == ".NET SDK");
        Assert.Equal(CheckSeverity.Info, sdkCheck.Severity);
        Assert.Contains("10.0.100", sdkCheck.Message);
        Assert.False(report.HasErrors);
    }

    [Fact]
    public async Task Validate_ReturnsNetSdkError_WhenNoSdkInstalled()
    {
        _runner.SetResult("dotnet", new ProcessResult(0, string.Empty, string.Empty));

        var report = await _validator.ValidateAsync(new EnvironmentValidationRequest());

        var sdkCheck = Assert.Single(report.Checks, c => c.Name == ".NET SDK");
        Assert.Equal(CheckSeverity.Error, sdkCheck.Severity);
        Assert.NotNull(sdkCheck.RecoveryCommands);
        Assert.NotEmpty(sdkCheck.RecoveryCommands);
        Assert.NotNull(sdkCheck.RecoveryUrl);
        Assert.True(report.HasErrors);
    }

    [Fact]
    public async Task Validate_ReturnsNetSdkError_WhenListSdksFails()
    {
        _runner.SetResult("dotnet", new ProcessResult(1, string.Empty, "dotnet not found"));

        var report = await _validator.ValidateAsync(new EnvironmentValidationRequest());

        var sdkCheck = Assert.Single(report.Checks, c => c.Name == ".NET SDK");
        Assert.Equal(CheckSeverity.Error, sdkCheck.Severity);
        Assert.True(report.HasErrors);
    }

    [Fact]
    public async Task Validate_DockerNotRequired_DoesNotCheckDocker()
    {
        _runner.SetResult("dotnet", new ProcessResult(0, "10.0.100 [x]", string.Empty));
        _runner.SetResult("docker", new ProcessResult(127, string.Empty, "docker not found"));

        var report = await _validator.ValidateAsync(new EnvironmentValidationRequest { RequireDocker = false });

        Assert.DoesNotContain(report.Checks, c => c.Name.StartsWith("Docker", StringComparison.OrdinalIgnoreCase));
        Assert.False(report.HasErrors);
    }

    [Fact]
    public async Task Validate_DockerCliMissing_ReturnsWarning()
    {
        _runner.SetResult("dotnet", new ProcessResult(0, "10.0.100 [x]", string.Empty));
        _runner.SetResult("docker", new ProcessResult(127, string.Empty, "docker not found"));

        var report = await _validator.ValidateAsync(new EnvironmentValidationRequest { RequireDocker = true });

        var dockerCheck = Assert.Single(report.Checks, c => c.Name == "Docker CLI");
        Assert.Equal(CheckSeverity.Warning, dockerCheck.Severity);
        Assert.False(report.HasErrors);
    }

    [Fact]
    public async Task Validate_DockerInstalledAndEngineRunning_ReturnsInfo()
    {
        // Both `docker --version` and `docker info` succeed.
        _runner.SetResult("dotnet", new ProcessResult(0, "10.0.100 [x]", string.Empty));
        _runner.SetResult("docker", new ProcessResult(0, "Docker version 27.0.0", string.Empty));

        var report = await _validator.ValidateAsync(new EnvironmentValidationRequest { RequireDocker = true });

        Assert.Contains(report.Checks, c => c.Name == "Docker CLI" && c.Severity == CheckSeverity.Info);
        Assert.Contains(report.Checks, c => c.Name == "Docker Engine" && c.Severity == CheckSeverity.Info);
        Assert.False(report.HasErrors);
    }

    [Fact]
    public async Task Validate_OutputDirectoryNotWritable_ReturnsError()
    {
        _runner.SetResult("dotnet", new ProcessResult(0, "10.0.100 [x]", string.Empty));
        _fileSystem.ThrowOnWrite = true;

        var report = await _validator.ValidateAsync(
            new EnvironmentValidationRequest { OutputDirectory = @"C:\some\dir" });

        var dirCheck = Assert.Single(report.Checks, c => c.Name == "Output directory");
        Assert.Equal(CheckSeverity.Error, dirCheck.Severity);
        Assert.True(report.HasErrors);
    }

    [Fact]
    public async Task Validate_OutputDirectoryWritable_ReturnsInfo()
    {
        _runner.SetResult("dotnet", new ProcessResult(0, "10.0.100 [x]", string.Empty));
        _fileSystem.CreateDirectory(@"C:\some\dir");

        var report = await _validator.ValidateAsync(
            new EnvironmentValidationRequest { OutputDirectory = @"C:\some\dir" });

        var dirCheck = Assert.Single(report.Checks, c => c.Name == "Output directory");
        Assert.Equal(CheckSeverity.Info, dirCheck.Severity);
        Assert.Contains(@"C:\some\dir", dirCheck.Message);
        Assert.False(report.HasErrors);
    }

    [Fact]
    public async Task Validate_InvalidSolutionName_ReturnsError()
    {
        _runner.SetResult("dotnet", new ProcessResult(0, "10.0.100 [x]", string.Empty));

        var report = await _validator.ValidateAsync(
            new EnvironmentValidationRequest { SolutionName = "123Bad" });

        var nameCheck = Assert.Single(report.Checks, c => c.Name == "Solution name");
        Assert.Equal(CheckSeverity.Error, nameCheck.Severity);
        Assert.True(report.HasErrors);
    }

    [Fact]
    public async Task Validate_ValidSolutionName_ReturnsInfo()
    {
        _runner.SetResult("dotnet", new ProcessResult(0, "10.0.100 [x]", string.Empty));

        var report = await _validator.ValidateAsync(
            new EnvironmentValidationRequest { SolutionName = "MyApp" });

        var nameCheck = Assert.Single(report.Checks, c => c.Name == "Solution name");
        Assert.Equal(CheckSeverity.Info, nameCheck.Severity);
        Assert.False(report.HasErrors);
    }
}
