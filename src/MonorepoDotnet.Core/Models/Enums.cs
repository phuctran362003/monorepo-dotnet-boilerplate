namespace MonorepoDotnet.Core.Models;

/// <summary>Supported target architectures.</summary>
public enum ArchitectureType
{
    Ddd = 0
}

/// <summary>Supported database providers.</summary>
public enum DatabaseProvider
{
    PostgreSql = 0
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
