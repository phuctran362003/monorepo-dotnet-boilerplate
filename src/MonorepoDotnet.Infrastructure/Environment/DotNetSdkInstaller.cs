using MonorepoDotnet.Core.Abstractions;
using MonorepoDotnet.Core.Generation;
using MonorepoDotnet.Core.Models;

namespace MonorepoDotnet.Infrastructure.Environment;

/// <summary>
/// Installs the .NET SDK from Microsoft: WinGet on Windows, otherwise the
/// official install script hosted at https://dot.net.
/// </summary>
public sealed class DotNetSdkInstaller : IDotNetSdkInstaller
{
    private readonly IProcessRunner _processRunner;

    public DotNetSdkInstaller(IProcessRunner processRunner)
    {
        _processRunner = processRunner;
    }

    public async Task<ProcessResult> InstallAsync(
        string targetFramework,
        CancellationToken cancellationToken = default)
    {
        var profile = DotNetVersionCatalog.Get(targetFramework);

        ProcessResult result;
        if (OperatingSystem.IsWindows())
        {
            result = await TryWingetAsync(profile, cancellationToken).ConfigureAwait(false);
            if (!result.Succeeded)
            {
                result = await RunOfficialWindowsScriptAsync(profile, cancellationToken).ConfigureAwait(false);
            }
        }
        else
        {
            result = await RunOfficialUnixScriptAsync(profile, cancellationToken).ConfigureAwait(false);
        }

        DotNetExecutable.PrependInstallLocationsToPath();
        return result;
    }

    private async Task<ProcessResult> TryWingetAsync(
        DotNetVersionProfile profile,
        CancellationToken cancellationToken)
    {
        try
        {
            return await _processRunner.RunAsync(
                    "winget",
                    [
                        "install",
                        "--id",
                        profile.WingetPackageId,
                        "-e",
                        "--accept-source-agreements",
                        "--accept-package-agreements",
                        "--disable-interactivity"
                    ],
                    cancellationToken: cancellationToken)
                .ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            return new ProcessResult(1, string.Empty, ex.Message);
        }
    }

    private Task<ProcessResult> RunOfficialWindowsScriptAsync(
        DotNetVersionProfile profile,
        CancellationToken cancellationToken) =>
        _processRunner.RunAsync(
            "powershell",
            [
                "-NoProfile",
                "-ExecutionPolicy",
                "Bypass",
                "-Command",
                "Invoke-WebRequest -Uri https://dot.net/v1/dotnet-install.ps1 -OutFile \"$env:TEMP\\dotnet-install.ps1\"; " +
                "& \"$env:TEMP\\dotnet-install.ps1\" -Channel " + profile.Channel
            ],
            cancellationToken: cancellationToken);

    private Task<ProcessResult> RunOfficialUnixScriptAsync(
        DotNetVersionProfile profile,
        CancellationToken cancellationToken) =>
        _processRunner.RunAsync(
            "bash",
            [
                "-lc",
                $"curl -sSL https://dot.net/v1/dotnet-install.sh | bash /dev/stdin --channel {profile.Channel}"
            ],
            cancellationToken: cancellationToken);
}
