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

    public ProjectKind ProjectKind { get; init; } = ProjectKind.WebApi;

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

    /// <summary>Default port of the selected database.</summary>
    public int DatabasePort => Database == DatabaseProvider.SqlServer ? 1433 : 5432;

    /// <summary>Database administrator used by the generated connection strings.</summary>
    public string DatabaseUser => Database == DatabaseProvider.SqlServer ? "sa" : "postgres";

    /// <summary>Docker Compose service name the API connects to.</summary>
    public string DatabaseService => Database == DatabaseProvider.SqlServer ? "sqlserver" : "postgres";

    public bool HasPackage(OptionalPackage package) => (Packages & package) == package;

    public string ApiProject => $"{SolutionName}.API";
    public string ServiceProject => $"{SolutionName}.Service";
    public string ConsoleProject => $"{SolutionName}.Console";
    public string WebApiProject => $"{SolutionName}.WebApi";
    public string ApplicationProject => $"{SolutionName}.Application";
    public string DomainProject => $"{SolutionName}.Domain";
    public string InfrastructureProject => $"{SolutionName}.Infrastructure";
    public string SingleLayerProject => SolutionName;
    public string MvcProject => $"{SolutionName}.Web";

    /// <summary>Project that hosts the application entry point for the selected architecture.</summary>
    public string MainProject => Architecture switch
    {
        ArchitectureType.Standard => ApiProject,
        ArchitectureType.Ddd => ProjectKind == ProjectKind.Console ? ConsoleProject : ApiProject,
        ArchitectureType.Clean => ProjectKind == ProjectKind.Console ? ConsoleProject : WebApiProject,
        ArchitectureType.SingleLayer => SingleLayerProject,
        ArchitectureType.Mvc => MvcProject,
        _ => throw new ArgumentOutOfRangeException(nameof(Architecture), Architecture, "Unsupported architecture.")
    };
}
