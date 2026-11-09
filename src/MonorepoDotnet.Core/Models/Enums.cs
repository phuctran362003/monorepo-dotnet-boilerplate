namespace MonorepoDotnet.Core.Models;

/// <summary>Host application chosen before the architecture.</summary>
public enum ProjectKind
{
    WebApi = 0,
    Mvc = 1,
    Console = 2
}

/// <summary>Supported target architectures.</summary>
public enum ArchitectureType
{
    Ddd = 0,
    Clean = 1,
    SingleLayer = 2,
    Mvc = 3
}

/// <summary>Supported database providers.</summary>
public enum DatabaseProvider
{
    PostgreSql = 0,
    SqlServer = 1
}

/// <summary>Optional packages that can be added to a generated solution.</summary>
[Flags]
public enum OptionalPackage
{
    None = 0,
    Serilog = 1 << 0,
    FluentValidation = 1 << 1,
    AutoMapper = 1 << 2,
    Redis = 1 << 3,
    MediatR = 1 << 4
}

public static class ProjectKindExtensions
{
    public static string ToDisplayName(this ProjectKind projectKind) => projectKind switch
    {
        ProjectKind.WebApi => "Web API",
        ProjectKind.Mvc => "MVC",
        ProjectKind.Console => "Console",
        _ => projectKind.ToString()
    };

    /// <summary>Prompt order. Web API stays first so it remains the default.</summary>
    public static IEnumerable<ProjectKind> All()
    {
        yield return ProjectKind.WebApi;
        yield return ProjectKind.Mvc;
        yield return ProjectKind.Console;
    }

    public static bool IsImplemented(this ProjectKind projectKind, ArchitectureType architecture) =>
        projectKind switch
        {
            ProjectKind.WebApi => architecture is ArchitectureType.Ddd or ArchitectureType.Clean or ArchitectureType.SingleLayer,
            ProjectKind.Mvc => architecture is ArchitectureType.Mvc,
            ProjectKind.Console => architecture is ArchitectureType.Ddd or ArchitectureType.Clean or ArchitectureType.SingleLayer,
            _ => false
        };
}

public static class ArchitectureTypeExtensions
{
    public static string ToDisplayName(this ArchitectureType architecture, ProjectKind projectKind) =>
        (projectKind, architecture) switch
        {
            (ProjectKind.WebApi, ArchitectureType.Ddd) => "DDD (Domain-Driven Design)",
            (ProjectKind.WebApi, ArchitectureType.Clean) => "Clean Architecture (Domain, Application, Infrastructure, WebApi)",
            (ProjectKind.WebApi, ArchitectureType.SingleLayer) => "1-Layer (single ASP.NET Core Web API project)",
            (ProjectKind.Mvc, ArchitectureType.Mvc) => "1-Layer (ASP.NET Core MVC with Razor views)",
            (ProjectKind.Console, ArchitectureType.Ddd) => "DDD (Domain-Driven Design)",
            (ProjectKind.Console, ArchitectureType.Clean) => "Clean Architecture (Domain, Application, Infrastructure, Console)",
            (ProjectKind.Console, ArchitectureType.SingleLayer) => "1-Layer (single .NET console project)",
            _ => architecture.ToString()
        };

    public static IEnumerable<ArchitectureType> For(ProjectKind projectKind)
    {
        switch (projectKind)
        {
            case ProjectKind.WebApi:
                yield return ArchitectureType.Ddd;
                yield return ArchitectureType.Clean;
                yield return ArchitectureType.SingleLayer;
                break;
            case ProjectKind.Mvc:
                yield return ArchitectureType.Mvc;
                break;
            case ProjectKind.Console:
                yield return ArchitectureType.Ddd;
                yield return ArchitectureType.Clean;
                yield return ArchitectureType.SingleLayer;
                break;
        }
    }
}

public static class DatabaseProviderExtensions
{
    public static string ToDisplayName(this DatabaseProvider provider) => provider switch
    {
        DatabaseProvider.PostgreSql => "PostgreSQL (EF Core, Code First)",
        DatabaseProvider.SqlServer => "SQL Server (EF Core, Code First)",
        _ => provider.ToString()
    };

    /// <summary>Short name used in the configuration summary and generated README files.</summary>
    public static string ToShortName(this DatabaseProvider provider) => provider switch
    {
        DatabaseProvider.PostgreSql => "PostgreSQL",
        DatabaseProvider.SqlServer => "SQL Server",
        _ => provider.ToString()
    };

    public static IEnumerable<DatabaseProvider> All()
    {
        yield return DatabaseProvider.PostgreSql;
        yield return DatabaseProvider.SqlServer;
    }
}

public static class OptionalPackageExtensions
{
    public static string ToDisplayName(this OptionalPackage package) => package switch
    {
        OptionalPackage.Serilog => "Serilog (structured logging)",
        OptionalPackage.FluentValidation => "FluentValidation (request validation)",
        OptionalPackage.AutoMapper => "AutoMapper (object mapping)",
        OptionalPackage.Redis => "Redis (distributed cache)",
        OptionalPackage.MediatR => "MediatR (in-process messaging)",
        _ => package.ToString()
    };

    public static IEnumerable<OptionalPackage> All()
    {
        yield return OptionalPackage.Serilog;
        yield return OptionalPackage.FluentValidation;
        yield return OptionalPackage.AutoMapper;
        yield return OptionalPackage.Redis;
        yield return OptionalPackage.MediatR;
    }
}
