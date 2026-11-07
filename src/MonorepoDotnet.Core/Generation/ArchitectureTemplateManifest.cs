using MonorepoDotnet.Core.Models;

namespace MonorepoDotnet.Core.Generation;

/// <summary>
/// Selects the template manifest for the requested architecture. Shared files
/// reuse the DDD templates; Clean Architecture only overrides the files that
/// differ (a simpler domain model and its mappings).
/// </summary>
public static class ArchitectureTemplateManifest
{
    public static IReadOnlyList<TemplateFile> For(GeneratorOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        return options.Architecture switch
        {
            ArchitectureType.Ddd => DddTemplateManifest.ForOptions(options).ToList(),
            ArchitectureType.Clean => CleanTemplateManifest.ForOptions(options).ToList(),
            ArchitectureType.SingleLayer => SingleLayerTemplateManifest.ForOptions(options).ToList(),
            ArchitectureType.Mvc => MvcTemplateManifest.ForOptions(options).ToList(),
            _ => throw new GenerationException(
                $"Architecture '{options.Architecture}' is not implemented yet.")
        };
    }
}
