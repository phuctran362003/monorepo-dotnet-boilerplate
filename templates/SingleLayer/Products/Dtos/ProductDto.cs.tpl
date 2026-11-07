using {{Namespace}}.Models;

namespace {{Namespace}}.Products.Dtos;

public sealed record ProductDto(
    Guid Id,
    string Name,
    decimal Price,
    string Status,
    DateTime CreatedAtUtc)
{
    public static ProductDto FromEntity(Product product) =>
        new(product.Id, product.Name, product.Price, product.Status, product.CreatedAtUtc);
}
