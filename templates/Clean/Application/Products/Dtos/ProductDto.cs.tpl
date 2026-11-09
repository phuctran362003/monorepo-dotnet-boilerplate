using {{DomainNamespace}}.Entities;

namespace {{ApplicationNamespace}}.Products.Dtos;

public sealed record ProductDto(
    Guid Id,
    string Name,
    decimal Price,
    string Status,
    DateTime CreatedAt)
{
    public static ProductDto FromEntity(Product product) =>
        new(product.Id, product.Name, product.Price, product.Status, product.CreatedAt);
}
