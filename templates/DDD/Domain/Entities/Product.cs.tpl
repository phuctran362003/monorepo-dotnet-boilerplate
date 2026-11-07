using {{DomainNamespace}}.Common;
using {{DomainNamespace}}.Enums;
using {{DomainNamespace}}.Exceptions;
using {{DomainNamespace}}.ValueObjects;

namespace {{DomainNamespace}}.Entities;

/// <summary>
/// Sample aggregate root. Replace or extend it with your own business model.
/// </summary>
public sealed class Product : AggregateRoot
{
    private Product()
    {
        // Required by EF Core.
    }

    private Product(ProductName name, decimal price, ProductStatus status)
    {
        Name = name;
        Price = price;
        Status = status;
    }

    public ProductName Name { get; private set; } = null!;

    public decimal Price { get; private set; }

    public ProductStatus Status { get; private set; }

    public DateTime CreatedAtUtc { get; private set; } = DateTime.UtcNow;

    public static Product Create(ProductName name, decimal price)
    {
        ArgumentNullException.ThrowIfNull(name);

        if (price < 0)
        {
            throw new DomainException("Product price cannot be negative.");
        }

        var product = new Product(name, price, ProductStatus.Draft);
        product.RaiseDomainEvent(new DomainEvents.ProductCreatedDomainEvent(product.Id, name.Value));
        return product;
    }

    public void UpdatePrice(decimal price)
    {
        if (price < 0)
        {
            throw new DomainException("Product price cannot be negative.");
        }

        Price = price;
    }

    public void Activate()
    {
        if (Status == ProductStatus.Discontinued)
        {
            throw new DomainException("A discontinued product cannot be activated.");
        }

        Status = ProductStatus.Active;
    }

    public void Discontinue() => Status = ProductStatus.Discontinued;
}
