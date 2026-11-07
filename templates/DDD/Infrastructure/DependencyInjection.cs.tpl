using {{ApplicationNamespace}}.Common.Interfaces;
{{#if Redis}}using {{ApplicationNamespace}}.Common.Caching;
{{/if}}using {{DomainNamespace}}.Repositories;
using {{InfrastructureNamespace}}.Persistence;
using {{InfrastructureNamespace}}.Persistence.Repositories;
{{#if Redis}}using {{InfrastructureNamespace}}.Caching;
{{/if}}using {{#if SqlServer}}Microsoft.Data.SqlClient;
using {{/if}}Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace {{InfrastructureNamespace}};

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

{{#if Postgres}}        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(connectionString, npgsql =>
            {
                npgsql.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName);
                npgsql.EnableRetryOnFailure(3);
            }));
{{/if}}{{#if SqlServer}}        var databasePassword = configuration["Database:Password"];

        if (!string.IsNullOrWhiteSpace(databasePassword))
        {
            var builder = new SqlConnectionStringBuilder(connectionString) { Password = databasePassword };
            connectionString = builder.ConnectionString;
        }

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(connectionString, sqlServer =>
            {
                sqlServer.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName);
                sqlServer.EnableRetryOnFailure(3);
            }));
{{/if}}        services.AddScoped<IProductRepository, ProductRepository>();
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
