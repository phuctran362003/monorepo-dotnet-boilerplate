using MonorepoDotnet.Core.Generation;
using MonorepoDotnet.Core.Models;
using MonorepoDotnet.Infrastructure.FileSystem;
using MonorepoDotnet.Infrastructure.Processes;
using MonorepoDotnet.Infrastructure.Templates;

namespace MonorepoDotnet.Tests;

public class ConsoleGenerationTests
{
    private static async Task<(GenerationResult Result, string Root)> GenerateConsoleAsync(
        ArchitectureType architecture,
        string solutionName,
        ProjectKind projectKind = ProjectKind.Console,
        bool includeDocker = true)
    {
        var output = Path.Combine(Path.GetTempPath(), $"monorepo-console-{Guid.NewGuid():N}");
        var root = Path.Combine(output, solutionName);

        var generator = new SolutionGenerator(
            new FileSystemService(),
            new TemplateProvider(),
            new TemplateRenderer(),
            new ProcessRunner());

        var result = await generator.GenerateAsync(new GeneratorOptions
        {
            SolutionName = solutionName,
            OutputDirectory = output,
            ProjectKind = projectKind,
            Architecture = architecture,
            Database = DatabaseProvider.PostgreSql,
            IncludeDocker = includeDocker,
            BuildAfterGeneration = true
        });

        return (result, root);
    }

    [Theory]
    [InlineData(ArchitectureType.Ddd, "ConsoleDdd")]
    [InlineData(ArchitectureType.Clean, "ConsoleClean")]
    [InlineData(ArchitectureType.SingleLayer, "ConsoleLayer")]
    public async Task Generate_ConsoleArchitecture_Builds(ArchitectureType architecture, string solutionName)
    {
        var (result, root) = await GenerateConsoleAsync(architecture, solutionName);

        Assert.True(result.Success, result.Error);
        Assert.True(File.Exists(Path.Combine(root, "Dockerfile")));
        Assert.Contains("OutputType>Exe<", File.ReadAllText(FindProject(root)), StringComparison.Ordinal);
        Assert.DoesNotContain(
            Directory.GetFiles(root, "*.cs", SearchOption.AllDirectories),
            path => path.Contains($"{Path.DirectorySeparatorChar}Controllers{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase));
        if (architecture != ArchitectureType.SingleLayer)
        {
            AssertPackagesLiveOnDomain(root);
        }
    }

    [Fact]
    public async Task Generate_DddWebApi_BuildsWithPackagesOnDomain()
    {
        var (result, root) = await GenerateConsoleAsync(
            ArchitectureType.Ddd, "WebDdd", projectKind: ProjectKind.WebApi, includeDocker: false);

        Assert.True(result.Success, result.Error);
        Assert.DoesNotContain(result.Warnings, warning => warning.Contains("could not be created", StringComparison.OrdinalIgnoreCase));
        var domainProject = File.ReadAllText(Path.Combine(root, "src", "WebDdd.Domain", "WebDdd.Domain.csproj"));
        Assert.Contains("Microsoft.AspNetCore.OpenApi", domainProject, StringComparison.Ordinal);
        Assert.Contains("Npgsql.EntityFrameworkCore.PostgreSQL", domainProject, StringComparison.Ordinal);
        AssertPackagesLiveOnDomain(root);
    }

    [Theory]
    [InlineData(ProjectKind.WebApi, ArchitectureType.Clean, "WebClean")]
    [InlineData(ProjectKind.WebApi, ArchitectureType.SingleLayer, "WebLayer")]
    [InlineData(ProjectKind.Mvc, ArchitectureType.Mvc, "WebMvc")]
    public async Task Generate_OtherHosts_Build(ProjectKind projectKind, ArchitectureType architecture, string solutionName)
    {
        var output = Path.Combine(Path.GetTempPath(), $"monorepo-other-{Guid.NewGuid():N}");
        var root = Path.Combine(output, solutionName);

        var generator = new SolutionGenerator(
            new FileSystemService(),
            new TemplateProvider(),
            new TemplateRenderer(),
            new ProcessRunner());

        var result = await generator.GenerateAsync(new GeneratorOptions
        {
            SolutionName = solutionName,
            OutputDirectory = output,
            ProjectKind = projectKind,
            Architecture = architecture,
            Database = DatabaseProvider.PostgreSql,
            IncludeDocker = false,
            BuildAfterGeneration = true
        });

        Assert.True(result.Success, result.Error);
        Assert.Contains("class BaseEntity", Directory.GetFiles(root, "BaseEntity.cs", SearchOption.AllDirectories).Select(File.ReadAllText).Single(), StringComparison.Ordinal);
    }

    private static void AssertPackagesLiveOnDomain(string root)
    {
        foreach (var project in Directory.GetFiles(root, "*.csproj", SearchOption.AllDirectories))
        {
            if (project.EndsWith(".Domain.csproj", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            Assert.DoesNotContain("PackageReference", File.ReadAllText(project), StringComparison.Ordinal);
        }
    }

    private static string FindProject(string root) =>
        Directory.GetFiles(root, "*.csproj", SearchOption.AllDirectories)
            .Single(path => File.ReadAllText(path).Contains("<OutputType>Exe</OutputType>", StringComparison.Ordinal));
}
