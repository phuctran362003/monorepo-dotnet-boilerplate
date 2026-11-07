{{#if SqlServer}}using Microsoft.Data.SqlClient;
{{/if}}using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace {{Namespace}}.Data;

/// <summary>
/// Design-time factory used by the EF Core CLI. Migrations can be created
/// without a running PostgreSQL.
/// </summary>
public sealed class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var connectionString =
            Environment.GetEnvironmentVariable("ConnectionStrings__Default")
{{#if Postgres}}            ?? "Host=localhost;Port={{DatabasePort}};Database={{DatabaseName}};Username={{DatabaseUser}}";
{{/if}}{{#if SqlServer}}            ?? "Server=localhost,{{DatabasePort}};Database={{DatabaseName}};User Id={{DatabaseUser}};TrustServerCertificate=True";

        var databasePassword = Environment.GetEnvironmentVariable("Database__Password");

        if (!string.IsNullOrWhiteSpace(databasePassword))
        {
            var builder = new SqlConnectionStringBuilder(connectionString) { Password = databasePassword };
            connectionString = builder.ConnectionString;
        }

{{/if}}        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
{{#if Postgres}}            .UseNpgsql(
                connectionString,
                npgsql => npgsql.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName))
{{/if}}{{#if SqlServer}}            .UseSqlServer(
                connectionString,
                sqlServer => sqlServer.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName))
{{/if}}            .Options;

        return new ApplicationDbContext(options);
    }
}
