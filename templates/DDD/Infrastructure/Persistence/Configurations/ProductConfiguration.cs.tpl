using {{Namespace}}.Domain.Entities;
using {{Namespace}}.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace {{Namespace}}.Infrastructure.Persistence.Configurations;

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

        builder.Property(product => product.CreatedAtUtc)
            .HasColumnName("created_at_utc")
            .IsRequired();

        builder.Ignore(product => product.DomainEvents);

        builder.HasIndex(product => product.Name).HasDatabaseName("ix_products_name");
    }
}
