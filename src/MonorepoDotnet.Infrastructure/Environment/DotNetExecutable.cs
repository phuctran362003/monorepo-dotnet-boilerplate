namespace MonorepoDotnet.Infrastructure.Environment;

/// <summary>Resolves the <c>dotnet</c> CLI after a PATH-based or Microsoft install.</summary>
internal static class DotNetExecutable
{
    public static string Resolve()
    {
        foreach (var candidate in CandidatePaths())
        {
            if (string.Equals(candidate, "dotnet", StringComparison.Ordinal))
            {
                return candidate;
            }

            if (File.Exists(candidate))
            {
                return candidate;
            }
        }

        return "dotnet";
    }

    public static void PrependInstallLocationsToPath()
    {
        var extras = InstallDirectories().Where(Directory.Exists).ToArray();
        if (extras.Length == 0)
        {
            return;
        }

        var current = System.Environment.GetEnvironmentVariable("PATH") ?? string.Empty;
        var prefix = string.Join(Path.PathSeparator, extras);
        System.Environment.SetEnvironmentVariable("PATH", prefix + Path.PathSeparator + current);
    }

    private static IEnumerable<string> CandidatePaths()
    {
        foreach (var directory in InstallDirectories())
        {
            yield return Path.Combine(directory, OperatingSystem.IsWindows() ? "dotnet.exe" : "dotnet");
        }

        yield return "dotnet";
    }

    private static IEnumerable<string> InstallDirectories()
    {
        var programFiles = System.Environment.GetFolderPath(System.Environment.SpecialFolder.ProgramFiles);
        if (!string.IsNullOrWhiteSpace(programFiles))
        {
            yield return Path.Combine(programFiles, "dotnet");
        }

        var localAppData = System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData);
        if (!string.IsNullOrWhiteSpace(localAppData))
        {
            yield return Path.Combine(localAppData, "Microsoft", "dotnet");
        }

        var home = System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile);
        if (!string.IsNullOrWhiteSpace(home))
        {
            yield return Path.Combine(home, ".dotnet");
        }

        yield return "/usr/share/dotnet";
        yield return "/usr/local/share/dotnet";
    }
}
