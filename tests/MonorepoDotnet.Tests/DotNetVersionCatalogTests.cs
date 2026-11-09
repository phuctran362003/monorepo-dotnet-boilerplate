using MonorepoDotnet.Core.Generation;
using MonorepoDotnet.Core.Validation;

namespace MonorepoDotnet.Tests;

public sealed class DotNetVersionCatalogTests
{
    [Theory]
    [InlineData("net8.0", 8, "8.0", "8.0.33")]
    [InlineData("net9.0", 9, "9.0", "9.0.22")]
    [InlineData("net10.0", 10, "10.0", "10.0.14")]
    public void Get_returns_matching_profile(
        string targetFramework,
        int major,
        string channel,
        string efCoreVersion)
    {
        var profile = DotNetVersionCatalog.Get(targetFramework);

        Assert.Equal(targetFramework, profile.TargetFramework);
        Assert.Equal(major, profile.Major);
        Assert.Equal(channel, profile.Channel);
        Assert.Equal(efCoreVersion, profile.EntityFrameworkCore);
        Assert.StartsWith("https://dotnet.microsoft.com/download/dotnet/", profile.DownloadUrl);
        Assert.Equal($"Microsoft.DotNet.SDK.{major}", profile.WingetPackageId);
    }

    [Fact]
    public void Unsupported_framework_is_rejected()
    {
        Assert.False(DotNetVersionCatalog.IsSupported("net7.0"));
        Assert.Throws<ArgumentException>(() => DotNetVersionCatalog.Get("net7.0"));
    }

    [Fact]
    public void ParseListSdks_reads_installed_versions()
    {
        const string output = """
            8.0.415 [C:\Program Files\dotnet\sdk]
            9.0.306 [C:\Program Files\dotnet\sdk]
            10.0.100 [C:\Program Files\dotnet\sdk]
            """;

        var versions = DotNetSdkVersionParser.ParseListSdks(output);

        Assert.Equal([new Version(8, 0, 415), new Version(9, 0, 306), new Version(10, 0, 100)], versions);
        Assert.True(DotNetSdkVersionParser.HasCompatibleSdk(versions, 8));
        Assert.True(DotNetSdkVersionParser.HasCompatibleSdk(versions, 10));
        Assert.False(DotNetSdkVersionParser.HasCompatibleSdk(versions, 11));
    }

    [Fact]
    public void ParseListSdks_treats_empty_output_as_no_sdk()
    {
        Assert.Empty(DotNetSdkVersionParser.ParseListSdks(""));
        Assert.False(DotNetSdkVersionParser.HasCompatibleSdk([], 8));
    }
}
