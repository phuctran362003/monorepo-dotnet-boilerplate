namespace MonorepoDotnet.Core.Models;

public enum CheckSeverity
{
    Info = 0,
    Warning = 1,
    Error = 2
}

/// <summary>Result of a single environment check.</summary>
public sealed record EnvironmentCheck(string Name, CheckSeverity Severity, string Message);

/// <summary>Aggregated environment validation result.</summary>
public sealed record EnvironmentReport(IReadOnlyList<EnvironmentCheck> Checks)
{
    public bool HasErrors => Checks.Any(c => c.Severity == CheckSeverity.Error);

    public bool HasWarnings => Checks.Any(c => c.Severity == CheckSeverity.Warning);

    public IEnumerable<EnvironmentCheck> Errors =>
        Checks.Where(c => c.Severity == CheckSeverity.Error);

    public IEnumerable<EnvironmentCheck> Warnings =>
        Checks.Where(c => c.Severity == CheckSeverity.Warning);
}

public sealed record EnvironmentValidationRequest
{
    public bool RequireDocker { get; init; }

    public string? OutputDirectory { get; init; }

    public string? SolutionName { get; init; }
}
