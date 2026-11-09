namespace MonorepoDotnet.Core.Distribution;

/// <summary>
/// Picks a newer stable NuGet version. <c>dotnet tool update</c> installs the
/// latest stable package unless <c>--prerelease</c> is passed, so prerelease
/// feeds are ignored.
/// </summary>
public static class NuGetVersionSet
{
    public static string? NewerStable(string currentVersion, IEnumerable<string> publishedVersions)
    {
        ArgumentNullException.ThrowIfNull(publishedVersions);

        if (!PackageVersion.TryParse(currentVersion, out var current))
        {
            return null;
        }

        string? latestText = null;
        PackageVersion? latest = null;

        foreach (var published in publishedVersions)
        {
            if (!PackageVersion.TryParse(published, out var candidate) || candidate.IsPrerelease)
            {
                continue;
            }

            if (latest is null || candidate.CompareTo(latest.Value) > 0)
            {
                latest = candidate;
                latestText = published.Trim();
            }
        }

        if (latest is null || latestText is null || latest.Value.CompareTo(current) <= 0)
        {
            return null;
        }

        return latestText;
    }

    private readonly record struct PackageVersion(int Major, int Minor, int Patch, int Revision, bool IsPrerelease)
        : IComparable<PackageVersion>
    {
        public static bool TryParse(string? text, out PackageVersion version)
        {
            version = default;

            if (string.IsNullOrWhiteSpace(text))
            {
                return false;
            }

            var value = text.Trim();
            var metadata = value.IndexOf('+');
            if (metadata >= 0)
            {
                value = value[..metadata];
            }

            var prerelease = value.IndexOf('-');
            var isPrerelease = prerelease >= 0;
            if (isPrerelease)
            {
                value = value[..prerelease];
            }

            var parts = value.Split('.');
            if (parts.Length is < 1 or > 4)
            {
                return false;
            }

            Span<int> numbers = stackalloc int[4];
            for (var index = 0; index < parts.Length; index++)
            {
                if (!int.TryParse(parts[index], out var number) || number < 0)
                {
                    return false;
                }

                numbers[index] = number;
            }

            version = new PackageVersion(numbers[0], numbers[1], numbers[2], numbers[3], isPrerelease);
            return true;
        }

        public int CompareTo(PackageVersion other)
        {
            var major = Major.CompareTo(other.Major);
            if (major != 0)
            {
                return major;
            }

            var minor = Minor.CompareTo(other.Minor);
            if (minor != 0)
            {
                return minor;
            }

            var patch = Patch.CompareTo(other.Patch);
            if (patch != 0)
            {
                return patch;
            }

            return Revision.CompareTo(other.Revision);
        }
    }
}
