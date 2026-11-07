namespace MonorepoDotnet.Core.Models;

/// <summary>
/// Everything required to generate a solution. The solution name is the single
/// source of truth for project names, namespaces, Docker services and images.
/// </summary>
public sealed record GeneratorOptions
{
    public const string DefaultDotNetVersion = "net10.0";

    public required string SolutionName { get; init; }

    public required string OutputDirectory { get; init; }

    public string DotNetVersion { get; init; } = DefaultDotNetVersion;

    public ArchitectureType Architecture { get; init; } = ArchitectureType.Ddd;

    public DatabaseProvider Database { get; init; } = DatabaseProvider.PostgreSql;

    public bool IncludeDocker { get; init; } = true;

    public OptionalPackage Packages { get; init; } = OptionalPackage.None;

    /// <summary>Restore and build the generated solution after generation.</summary>
    public bool BuildAfterGeneration { get; init; } = true;

    /// <summary>Absolute path of the generated solution directory.</summary>
    public string RootDirectory => Path.GetFullPath(Path.Combine(OutputDirectory, SolutionName));

    /// <summary>Lower-case name used for Docker services, images and database names.</summary>
    public string DockerName => SolutionName.ToLowerInvariant();

    public bool HasPackage(OptionalPackage package) => (Packages & package) == package;

    public string ApiProject => $"{SolutionName}.API";
    public string ApplicationProject => $"{SolutionName}.Application";
    public string DomainProject => $"{SolutionName}.Domain";
    public string InfrastructureProject => $"{SolutionName}.Infrastructure";
}
