using MonorepoDotnet.Core.Generation;
using MonorepoDotnet.Core.Models;
using MonorepoDotnet.Infrastructure.FileSystem;
using MonorepoDotnet.Infrastructure.Processes;
using MonorepoDotnet.Infrastructure.Templates;

namespace MonorepoDotnet.Tests;

public class StandardGenerationTests
{
    [Fact]
    public async Task Generate_StandardBackend_BuildsTheDocumentedLayout()
    {
        var output = Path.Combine(Path.GetTempPath(), "monorepo-standard-check");
        var root = Path.Combine(output, "Shop");
        if (Directory.Exists(root))
        {
            Directory.Delete(root, recursive: true);
        }

        var generator = new SolutionGenerator(
            new FileSystemService(),
            new TemplateProvider(),
            new TemplateRenderer(),
            new ProcessRunner());

        var result = await generator.GenerateAsync(new GeneratorOptions
        {
            SolutionName = "Shop",
            OutputDirectory = output,
            ProjectKind = ProjectKind.WebApi,
            Architecture = ArchitectureType.Standard,
            Database = DatabaseProvider.PostgreSql,
            IncludeDocker = true,
            BuildAfterGeneration = true
        });

        Assert.True(result.Success, result.Error);
        Assert.True(File.Exists(Path.Combine(root, "Shop.slnx")));
        Assert.True(File.Exists(Path.Combine(root, "src", "Shop.API", "Architecture", "IOCContainer.cs")));
        Assert.True(File.Exists(Path.Combine(root, "src", "Shop.Service", "Utils", "ApiResult.cs")));
        Assert.True(File.Exists(Path.Combine(root, "src", "Shop.Infrastructure", "UnitOfWork.cs")));
        Assert.True(File.Exists(Path.Combine(root, "src", "Shop.Domain", "ShopDbContext.cs")));
        Assert.Contains("<ProjectReference Include=\"..\\Shop.Service\\Shop.Service.csproj\" />", File.ReadAllText(Path.Combine(root, "src", "Shop.API", "Shop.API.csproj")));
        Assert.Contains("<ProjectReference Include=\"..\\Shop.Infrastructure\\Shop.Infrastructure.csproj\" />", File.ReadAllText(Path.Combine(root, "src", "Shop.Service", "Shop.Service.csproj")));
        Assert.Contains("<ProjectReference Include=\"..\\Shop.Domain\\Shop.Domain.csproj\" />", File.ReadAllText(Path.Combine(root, "src", "Shop.Infrastructure", "Shop.Infrastructure.csproj")));
        Assert.DoesNotContain("ProjectReference", File.ReadAllText(Path.Combine(root, "src", "Shop.Domain", "Shop.Domain.csproj")));
    }
}
