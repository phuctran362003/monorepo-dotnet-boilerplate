using {{DomainNamespace}}.Entities;
using {{DomainNamespace}}.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace {{InfrastructureNamespace}}.Persistence.Configurations;

public sealed class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("products");

        builder.HasKey(product => product.Id);
        builder.Property(product => product.Id).ValueGeneratedNever();

        builder.Property(product => product.Name)
            .HasConversion(name => name.Value, value => ProductName.Create(value))
            .HasColumnName("name")
            .HasMaxLength(ProductName.MaxLength)
            .IsRequired();

        builder.Property(product => product.Price)
            .HasColumnName("price")
            .HasPrecision(18, 2);

        builder.Property(product => product.Status)
            .HasConversion<string>()
            .HasColumnName("status")
            .HasMaxLength(32)
            .IsRequired();

        builder.HasQueryFilter(product => !product.IsDeleted);

        builder.Ignore(product => product.DomainEvents);

        builder.HasIndex(product => product.Name).HasDatabaseName("ix_products_name");
    }
}
