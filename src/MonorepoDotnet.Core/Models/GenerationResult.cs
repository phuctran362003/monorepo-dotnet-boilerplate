namespace MonorepoDotnet.Core.Models;

/// <summary>A single step of the generation pipeline.</summary>
public sealed record GenerationProgress(int Step, int TotalSteps, string Message);

/// <summary>Outcome of a generation run.</summary>
public sealed record GenerationResult
{
    public required bool Success { get; init; }

    public required string RootDirectory { get; init; }

    public string? Error { get; init; }

    public IReadOnlyList<string> Warnings { get; init; } = [];

    public static GenerationResult Ok(string rootDirectory, IReadOnlyList<string>? warnings = null) =>
        new() { Success = true, RootDirectory = rootDirectory, Warnings = warnings ?? [] };

    public static GenerationResult Failed(string rootDirectory, string error) =>
        new() { Success = false, RootDirectory = rootDirectory, Error = error };
}
