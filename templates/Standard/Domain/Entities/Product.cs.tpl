namespace {{DomainNamespace}}.Entities;

public class Product : BaseEntity
{
    public string Name { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public string? Description { get; set; }
}
