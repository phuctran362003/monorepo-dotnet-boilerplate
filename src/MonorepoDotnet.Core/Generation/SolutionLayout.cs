using MonorepoDotnet.Core.Models;

namespace MonorepoDotnet.Core.Generation;

/// <summary>A project that is added to the generated solution file.</summary>
/// <param name="Name">Project file name without extension.</param>
/// <param name="RelativePath">Path relative to the solution root.</param>
public sealed record SolutionProject(string Name, string RelativePath);

/// <summary>
/// Describes the project layout produced by each architecture. The generator
/// uses it for the solution file, Dockerfile and EF Core migration commands.
/// </summary>
public sealed record SolutionLayout(
    ArchitectureType Architecture,
    IReadOnlyList<SolutionProject> Projects,
    SolutionProject MainProject,
    SolutionProject? MigrationProject,
    SolutionProject? MigrationStartupProject,
    string? MigrationOutputDirectory);

public static class SolutionLayouts
{
    public static SolutionLayout Create(GeneratorOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        return options.Architecture switch
        {
            ArchitectureType.Ddd => CreateDdd(options),
            ArchitectureType.Clean => CreateClean(options),
            ArchitectureType.SingleLayer => CreateSingleProject(
                options,
                ArchitectureType.SingleLayer,
                options.SingleLayerProject,
                "Data/Migrations"),
            ArchitectureType.Mvc => CreateSingleProject(
                options,
                ArchitectureType.Mvc,
                options.MvcProject,
                "Data/Migrations"),
            _ => throw new GenerationException(
                $"Architecture '{options.Architecture}' is not implemented yet.")
        };
    }

    private static SolutionLayout CreateDdd(GeneratorOptions options)
    {
        var domain = Project(options.DomainProject);
        var application = Project(options.ApplicationProject);
        var infrastructure = Project(options.InfrastructureProject);
        var host = Project(options.MainProject);

        return new SolutionLayout(
            ArchitectureType.Ddd,
            [domain, application, infrastructure, host],
            host,
            infrastructure,
            host,
            "Persistence/Migrations");
    }

    private static SolutionLayout CreateClean(GeneratorOptions options)
    {
        var domain = Project(options.DomainProject);
        var application = Project(options.ApplicationProject);
        var infrastructure = Project(options.InfrastructureProject);
        var host = Project(options.MainProject);

        return new SolutionLayout(
            ArchitectureType.Clean,
            [domain, application, infrastructure, host],
            host,
            infrastructure,
            host,
            "Persistence/Migrations");
    }

    private static SolutionLayout CreateSingleProject(
        GeneratorOptions options,
        ArchitectureType architecture,
        string projectName,
        string migrationDirectory)
    {
        var project = Project(projectName);

        return new SolutionLayout(
            architecture,
            [project],
            project,
            project,
            project,
            migrationDirectory);
    }

    private static SolutionProject Project(string name) =>
        new(name, $"src/{name}/{name}.csproj");
}
