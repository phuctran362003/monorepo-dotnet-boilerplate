using {{Namespace}}.Exceptions;

namespace {{Namespace}}.Models;

/// <summary>
/// Sample entity for the 1-layer layout. Keep business rules on the entity so
/// controllers and services stay thin.
/// </summary>
public sealed class Product
{
    private Product()
    {
        // Required by EF Core.
    }

    private Product(string name, decimal price)
    {
        Name = name;
        Price = price;
        Status = "Draft";
        CreatedAtUtc = DateTime.UtcNow;
    }

    public Guid Id { get; private set; } = Guid.NewGuid();

    public string Name { get; private set; } = string.Empty;

    public decimal Price { get; private set; }

    public string Status { get; private set; } = "Draft";

    public DateTime CreatedAtUtc { get; private set; }

    public static Product Create(string name, decimal price)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("Product name is required.");
        }

        if (name.Trim().Length > 200)
        {
            throw new DomainException("Product name cannot be longer than 200 characters.");
        }

        if (price < 0)
        {
            throw new DomainException("Product price cannot be negative.");
        }

        return new Product(name.Trim(), price);
    }

    public void UpdatePrice(decimal price)
    {
        if (price < 0)
        {
            throw new DomainException("Product price cannot be negative.");
        }

        Price = price;
    }

    public void Activate() => Status = "Active";

    public void Discontinue() => Status = "Discontinued";
}
