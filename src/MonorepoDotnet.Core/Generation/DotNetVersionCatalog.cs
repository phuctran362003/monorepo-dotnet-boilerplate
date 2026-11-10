namespace MonorepoDotnet.Core.Generation;

/// <summary>
/// Target framework for generated solutions, including the NuGet versions and
/// Microsoft install metadata that go with it.
/// </summary>
public sealed record DotNetVersionProfile(
    string TargetFramework,
    string DisplayName,
    int Major,
    string Channel,
    string ImageTag,
    string EntityFrameworkCore,
    string NpgsqlEntityFrameworkCore,
    string AspNetCoreOpenApi,
    string MicrosoftExtensions,
    string CachingRedis,
    string SerilogAspNetCore,
    string JwtBearer,
    string AspNetCoreOData,
    string FluentValidationAspNetCore)
{
    /// <summary>
    /// Version for Microsoft.Extensions.DependencyInjection/Logging.Abstractions.
    /// </summary>
    public string ExtensionsAbstractions => MicrosoftExtensions;

    public string WingetPackageId => $"Microsoft.DotNet.SDK.{Major}";

    public string DownloadUrl => $"https://dotnet.microsoft.com/download/dotnet/{Channel}";
}

public static class DotNetVersionCatalog
{
    public static readonly DotNetVersionProfile Net10 = new(
        TargetFramework: "net10.0",
        DisplayName: ".NET 10",
        Major: 10,
        Channel: "10.0",
        ImageTag: "10.0",
        EntityFrameworkCore: "10.0.14",
        NpgsqlEntityFrameworkCore: "10.0.3",
        AspNetCoreOpenApi: "10.0.14",
        MicrosoftExtensions: "10.0.14",
        CachingRedis: "10.0.14",
        SerilogAspNetCore: "10.0.0",
        JwtBearer: "10.0.14",
        AspNetCoreOData: "9.5.0",
        FluentValidationAspNetCore: "11.3.1");

    /// <summary>Generated solutions target .NET 10 only.</summary>
    public static IReadOnlyList<DotNetVersionProfile> All { get; } = [Net10];

    public static bool IsSupported(string targetFramework) =>
        All.Any(profile => profile.TargetFramework.Equals(targetFramework, StringComparison.Ordinal));

    public static DotNetVersionProfile Get(string targetFramework) =>
        All.FirstOrDefault(profile => profile.TargetFramework.Equals(targetFramework, StringComparison.Ordinal))
        ?? throw new ArgumentException($"Target framework '{targetFramework}' is not supported.", nameof(targetFramework));

    public static IReadOnlyList<string> GetInstallCommands(DotNetVersionProfile profile)
    {
        if (OperatingSystem.IsWindows())
        {
            return
            [
                $"winget install --id {profile.WingetPackageId} -e --accept-source-agreements --accept-package-agreements",
                "powershell -NoProfile -ExecutionPolicy Bypass -Command \"Invoke-WebRequest -Uri https://dot.net/v1/dotnet-install.ps1 -OutFile $env:TEMP\\dotnet-install.ps1; & $env:TEMP\\dotnet-install.ps1 -Channel " + profile.Channel + "\""
            ];
        }

        return
        [
            $"curl -sSL https://dot.net/v1/dotnet-install.sh | bash /dev/stdin --channel {profile.Channel}"
        ];
    }
}
