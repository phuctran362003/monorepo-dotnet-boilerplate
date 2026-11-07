using {{Namespace}}.Application.Common.Interfaces;
{{#if Redis}}using {{Namespace}}.Application.Common.Caching;
{{/if}}using {{Namespace}}.Domain.Repositories;
using {{Namespace}}.Infrastructure.Persistence;
using {{Namespace}}.Infrastructure.Persistence.Repositories;
{{#if Redis}}using {{Namespace}}.Infrastructure.Caching;
{{/if}}using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace {{Namespace}}.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        var connectionString = configuration.GetConnectionString("Default");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "Connection string 'Default' is not configured. Set the " +
                "ConnectionStrings__Default environment variable or appsettings.json.");
        }

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(connectionString, npgsql =>
            {
                npgsql.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName);
                npgsql.EnableRetryOnFailure(3);
            }));

        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<ApplicationDbContext>());
{{#if Redis}}
        var redisConnection = configuration.GetConnectionString("Redis") ?? "localhost:6379";

        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = redisConnection;
            options.InstanceName = "{{DatabaseName}}:";
        });

        services.AddSingleton<ICacheService, RedisCacheService>();
{{/if}}
        return services;
    }
}
