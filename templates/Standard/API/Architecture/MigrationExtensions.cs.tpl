using {{DomainNamespace}};
using Microsoft.EntityFrameworkCore;

namespace {{ApiNamespace}}.Architecture;

public static class MigrationExtensions
{
    public static void ApplyMigrations(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<{{SolutionName}}DbContext>();
        dbContext.Database.Migrate();
    }
}
