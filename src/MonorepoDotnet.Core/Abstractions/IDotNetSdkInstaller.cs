using MonorepoDotnet.Core.Models;

namespace MonorepoDotnet.Core.Abstractions;

/// <summary>
/// Installs a .NET SDK from Microsoft (WinGet on Windows, otherwise the
/// official <c>dotnet-install</c> script from https://dot.net).
/// </summary>
public interface IDotNetSdkInstaller
{
    Task<ProcessResult> InstallAsync(
        string targetFramework,
        CancellationToken cancellationToken = default);
}
