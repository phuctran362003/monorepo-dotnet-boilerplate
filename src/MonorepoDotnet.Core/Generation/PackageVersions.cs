namespace MonorepoDotnet.Core.Generation;

/// <summary>
/// Pinned, stable package versions used by generated solutions. Centralizing
/// them here keeps every template consistent and makes upgrades a one-file
/// change.
/// </summary>
public static class PackageVersions
{
    public const string DotNetImageTag = "10.0";
    public const string EntityFrameworkCore = "10.0.14";
    public const string NpgsqlEntityFrameworkCore = "10.0.3";
    public const string AspNetCoreOpenApi = "10.0.14";
    public const string MicrosoftExtensions = "10.0.14";
    public const string SerilogAspNetCore = "10.0.0";
    public const string FluentValidation = "12.2.0";
    public const string AutoMapper = "16.3.0";
    public const string MediatR = "12.5.0";
    public const string StackExchangeRedis = "3.4.31";
    public const string PostgresImage = "17-alpine";
    public const string SqlServerImage = "2022-latest";
    public const string RedisImage = "7-alpine";
}
