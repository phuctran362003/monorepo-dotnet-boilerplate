using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace {{Namespace}}.Infrastructure.Persistence;

/// <summary>
/// Design-time factory used by the EF Core CLI. Migrations can be created
/// without a running PostgreSQL; commands that do connect (database update)
/// read the connection string from the ConnectionStrings__Default environment
/// variable, exactly like the API does.
/// </summary>
public sealed class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var connectionString =
            Environment.GetEnvironmentVariable("ConnectionStrings__Default")
            ?? "Host=localhost;Port={{PostgresPort}};Database={{DatabaseName}};Username={{PostgresUser}}";

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql(
                connectionString,
                npgsql => npgsql.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName))
            .Options;

        return new ApplicationDbContext(options);
    }
}
