namespace MonorepoDotnet.Core.Generation;

/// <summary>
/// Supported target frameworks for generated solutions, including the NuGet
/// versions and Microsoft install metadata that go with each one.
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
    string SerilogAspNetCore)
{
    public string WingetPackageId => $"Microsoft.DotNet.SDK.{Major}";

    public string DownloadUrl => $"https://dotnet.microsoft.com/download/dotnet/{Channel}";

    public string ChoiceLabel => $"{TargetFramework} ({DisplayName})";
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
        SerilogAspNetCore: "10.0.0");

    public static readonly DotNetVersionProfile Net9 = new(
        TargetFramework: "net9.0",
        DisplayName: ".NET 9",
        Major: 9,
        Channel: "9.0",
        ImageTag: "9.0",
        EntityFrameworkCore: "9.0.22",
        NpgsqlEntityFrameworkCore: "9.0.4",
        AspNetCoreOpenApi: "9.0.22",
        MicrosoftExtensions: "9.0.22",
        CachingRedis: "9.0.22",
        SerilogAspNetCore: "9.0.0");

    public static readonly DotNetVersionProfile Net8 = new(
        TargetFramework: "net8.0",
        DisplayName: ".NET 8",
        Major: 8,
        Channel: "8.0",
        ImageTag: "8.0",
        EntityFrameworkCore: "8.0.33",
        NpgsqlEntityFrameworkCore: "8.0.11",
        AspNetCoreOpenApi: "8.0.33",
        MicrosoftExtensions: "8.0.0",
        CachingRedis: "8.0.22",
        SerilogAspNetCore: "8.0.3");

    /// <summary>Prompt order: newest first so .NET 10 remains the default.</summary>
    public static IReadOnlyList<DotNetVersionProfile> All { get; } = [Net10, Net9, Net8];

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
