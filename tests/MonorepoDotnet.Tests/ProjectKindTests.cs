using MonorepoDotnet.Core.Models;

namespace MonorepoDotnet.Tests;

public class ProjectKindTests
{
    [Fact]
    public void For_WebApi_OffersDddCleanAndSingleLayer()
    {
        var architectures = ArchitectureTypeExtensions.For(ProjectKind.WebApi).ToArray();

        Assert.Equal([ArchitectureType.Ddd, ArchitectureType.Clean, ArchitectureType.SingleLayer], architectures);
        Assert.All(architectures, architecture => Assert.True(ProjectKind.WebApi.IsImplemented(architecture)));
    }

    [Fact]
    public void For_Mvc_OffersTheRazorLayout()
    {
        var architectures = ArchitectureTypeExtensions.For(ProjectKind.Mvc).ToArray();

        Assert.Equal([ArchitectureType.Mvc], architectures);
        Assert.True(ProjectKind.Mvc.IsImplemented(ArchitectureType.Mvc));
    }

    [Fact]
    public void For_Console_OffersArchitecturesThatAreNotImplemented()
    {
        var architectures = ArchitectureTypeExtensions.For(ProjectKind.Console).ToArray();

        Assert.Equal([ArchitectureType.Ddd, ArchitectureType.Clean, ArchitectureType.SingleLayer], architectures);
        Assert.All(architectures, architecture => Assert.False(ProjectKind.Console.IsImplemented(architecture)));
    }
}
