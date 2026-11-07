using MonorepoDotnet.Infrastructure.Templates;

namespace MonorepoDotnet.Tests;

public class TemplateRendererTests
{
    [Fact]
    public void Render_SimpleVariableSubstitution_Succeeds()
    {
        var renderer = new TemplateRenderer();
        var template = "Hello {{SolutionName}} with {{TargetFramework}}!";
        var values = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["SolutionName"] = "MyApp",
            ["TargetFramework"] = "net10.0"
        };
        var flags = new Dictionary<string, bool>(StringComparer.Ordinal);

        var result = renderer.Render(template, values, flags);

        Assert.Equal("Hello MyApp with net10.0!", result);
    }

    [Fact]
    public void Render_ConditionalBlocks_Succeeds()
    {
        var renderer = new TemplateRenderer();
        var template = "Base{{#if Serilog}} with Serilog{{/if}}{{#if Unused}} unused{{/if}}!";
        var values = new Dictionary<string, string>(StringComparer.Ordinal);
        var flags = new Dictionary<string, bool>(StringComparer.Ordinal)
        {
            ["Serilog"] = true,
            ["Unused"] = false
        };

        var result = renderer.Render(template, values, flags);

        Assert.Equal("Base with Serilog!", result);
    }
}
