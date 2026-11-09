namespace MonorepoDotnet.Core.Generation;

/// <summary>
/// Pinned, stable package versions used by generated solutions. Framework-
/// specific versions come from <see cref="DotNetVersionCatalog"/>; shared
/// packages live here so upgrades stay a one-file change.
/// </summary>
public static class PackageVersions
{
    public const string FluentValidation = "12.2.0";
    public const string AutoMapper = "16.3.0";
    public const string MediatR = "12.5.0";
    public const string SwashbuckleSwaggerUi = "10.3.1";
    public const string StackExchangeRedis = "3.4.31";
    public const string PostgresImage = "17-alpine";
    public const string SqlServerImage = "2022-latest";
    public const string RedisImage = "7-alpine";

    public static DotNetVersionProfile For(string targetFramework) =>
        DotNetVersionCatalog.Get(targetFramework);
}
