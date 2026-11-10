using MonorepoDotnet.Core.Generation;
using MonorepoDotnet.Core.Models;

namespace MonorepoDotnet.Tests;

public class SolutionLayoutTests
{
    [Fact]
    public void Create_StandardArchitecture_UsesTheBackendStructureProjects()
    {
        var options = new GeneratorOptions
        {
            SolutionName = "TestApp",
            OutputDirectory = Path.GetTempPath(),
            Architecture = ArchitectureType.Standard,
            Database = DatabaseProvider.PostgreSql
        };

        var layout = SolutionLayouts.Create(options);

        Assert.Equal("TestApp.API", layout.MainProject.Name);
        Assert.Equal("TestApp.Domain", layout.MigrationProject!.Name);
        Assert.Equal("TestApp.API", layout.MigrationStartupProject!.Name);
        Assert.Equal("Migrations", layout.MigrationOutputDirectory);
        Assert.Equal(
            ["TestApp.Domain", "TestApp.Infrastructure", "TestApp.Service", "TestApp.API"],
            layout.Projects.Select(project => project.Name).ToArray());
    }

    [Fact]
    public void Create_DddArchitecture_ReturnsCorrectProjects()
    {
        var options = new GeneratorOptions
        {
            SolutionName = "TestApp",
            OutputDirectory = Path.GetTempPath(),
            Architecture = ArchitectureType.Ddd,
            Database = DatabaseProvider.PostgreSql
        };

        var layout = SolutionLayouts.Create(options);

        Assert.Equal("TestApp.API", layout.MainProject.Name);
        Assert.NotNull(layout.MigrationProject);
        Assert.Equal("TestApp.Infrastructure", layout.MigrationProject.Name);
        Assert.Equal(4, layout.Projects.Count);
        Assert.Contains(layout.Projects, p => p.Name == "TestApp.Domain");
        Assert.Contains(layout.Projects, p => p.Name == "TestApp.Application");
        Assert.Contains(layout.Projects, p => p.Name == "TestApp.Infrastructure");
        Assert.Contains(layout.Projects, p => p.Name == "TestApp.API");
    }

    [Fact]
    public void Create_CleanArchitecture_ReturnsCorrectProjects()
    {
        var options = new GeneratorOptions
        {
            SolutionName = "TestApp",
            OutputDirectory = Path.GetTempPath(),
            Architecture = ArchitectureType.Clean,
            Database = DatabaseProvider.PostgreSql
        };

        var layout = SolutionLayouts.Create(options);

        Assert.Equal("TestApp.WebApi", layout.MainProject.Name);
        Assert.NotNull(layout.MigrationProject);
        Assert.Equal(4, layout.Projects.Count);
        Assert.Contains(layout.Projects, p => p.Name == "TestApp.Domain");
        Assert.Contains(layout.Projects, p => p.Name == "TestApp.Application");
        Assert.Contains(layout.Projects, p => p.Name == "TestApp.Infrastructure");
        Assert.Contains(layout.Projects, p => p.Name == "TestApp.WebApi");
    }

    [Fact]
    public void Create_SingleLayerArchitecture_ReturnsSingleProject()
    {
        var options = new GeneratorOptions
        {
            SolutionName = "TestApp",
            OutputDirectory = Path.GetTempPath(),
            Architecture = ArchitectureType.SingleLayer,
            Database = DatabaseProvider.PostgreSql
        };

        var layout = SolutionLayouts.Create(options);

        Assert.Equal("TestApp", layout.MainProject.Name);
        Assert.NotNull(layout.MigrationProject);
        Assert.Equal("TestApp", layout.MigrationProject.Name);
        Assert.Single(layout.Projects);
    }

    [Fact]
    public void Create_MvcArchitecture_ReturnsMvcProject()
    {
        var options = new GeneratorOptions
        {
            SolutionName = "TestApp",
            OutputDirectory = Path.GetTempPath(),
            Architecture = ArchitectureType.Mvc,
            Database = DatabaseProvider.PostgreSql
        };

        var layout = SolutionLayouts.Create(options);

        Assert.Equal("TestApp.Web", layout.MainProject.Name);
        Assert.NotNull(layout.MigrationProject);
        Assert.Equal("TestApp.Web", layout.MigrationProject.Name);
        Assert.Single(layout.Projects);
    }

    [Theory]
    [InlineData(ArchitectureType.Ddd, "TestApp.Console")]
    [InlineData(ArchitectureType.Clean, "TestApp.Console")]
    [InlineData(ArchitectureType.SingleLayer, "TestApp")]
    public void Create_ConsoleHost_UsesConsoleEntryProject(ArchitectureType architecture, string mainProject)
    {
        var options = new GeneratorOptions
        {
            SolutionName = "TestApp",
            OutputDirectory = Path.GetTempPath(),
            ProjectKind = ProjectKind.Console,
            Architecture = architecture,
            Database = DatabaseProvider.PostgreSql
        };

        var layout = SolutionLayouts.Create(options);

        Assert.Equal(mainProject, layout.MainProject.Name);
        Assert.Equal(mainProject, layout.MigrationStartupProject?.Name);
    }
}
