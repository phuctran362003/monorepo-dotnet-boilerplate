using {{DomainNamespace}}.Entities;
using Microsoft.EntityFrameworkCore;

namespace {{DomainNamespace}};

public class {{SolutionName}}DbContext : DbContext
{
    public {{SolutionName}}DbContext()
    {
    }

    public {{SolutionName}}DbContext(DbContextOptions<{{SolutionName}}DbContext> options)
        : base(options)
    {
    }

    public DbSet<Product> Products => Set<Product>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(product => product.Id);
            entity.Property(product => product.Name).IsRequired().HasMaxLength(200);
            entity.Property(product => product.Price).IsRequired();
            entity.Property(product => product.Description).HasMaxLength(2000);
            entity.HasIndex(product => product.Name);
        });
    }
}
