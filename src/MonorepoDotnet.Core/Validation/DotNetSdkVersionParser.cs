namespace MonorepoDotnet.Core.Validation;

/// <summary>Parses <c>dotnet --list-sdks</c> output.</summary>
public static class DotNetSdkVersionParser
{
    public static IReadOnlyList<Version> ParseListSdks(string standardOutput)
    {
        if (string.IsNullOrWhiteSpace(standardOutput))
        {
            return [];
        }

        var versions = new List<Version>();

        foreach (var line in standardOutput.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries))
        {
            var token = line.Trim().Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
            if (token.Length == 0)
            {
                continue;
            }

            var numeric = token[0].Split('-')[0];
            if (Version.TryParse(numeric, out var version))
            {
                versions.Add(version);
            }
        }

        return versions;
    }

    public static bool HasCompatibleSdk(IReadOnlyList<Version> installedSdks, int requiredMajor) =>
        installedSdks.Any(sdk => sdk.Major >= requiredMajor);
}
