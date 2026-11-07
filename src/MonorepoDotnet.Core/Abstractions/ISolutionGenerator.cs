using MonorepoDotnet.Core.Models;

namespace MonorepoDotnet.Core.Abstractions;

/// <summary>Generates a solution from a validated <see cref="GeneratorOptions"/>.</summary>
public interface ISolutionGenerator
{
    Task<GenerationResult> GenerateAsync(
        GeneratorOptions options,
        IProgress<GenerationProgress>? progress = null,
        CancellationToken cancellationToken = default);
}
