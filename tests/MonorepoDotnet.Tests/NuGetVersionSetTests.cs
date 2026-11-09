using MonorepoDotnet.Core.Distribution;

namespace MonorepoDotnet.Tests;

public class NuGetVersionSetTests
{
    [Fact]
    public void NewerStable_ReturnsTheHighestStableVersion()
    {
        var latest = NuGetVersionSet.NewerStable("0.1.4", ["0.1.4", "0.1.10", "0.2.0-beta", "0.1.5"]);

        Assert.Equal("0.1.10", latest);
    }

    [Fact]
    public void NewerStable_ReturnsNullWhenCurrentIsCurrentOrNewer()
    {
        Assert.Null(NuGetVersionSet.NewerStable("0.1.4", ["0.1.4", "0.1.3"]));
        Assert.Null(NuGetVersionSet.NewerStable("0.2.0", ["0.1.9"]));
    }

    [Fact]
    public void NewerStable_IgnoresPrereleaseAndUnreadableVersions()
    {
        Assert.Null(NuGetVersionSet.NewerStable("0.1.4", ["0.2.0-preview.1", "not-a-version"]));
        Assert.Null(NuGetVersionSet.NewerStable("latest", ["0.1.5"]));
    }
}