using {{Namespace}}.Domain.Entities;

namespace {{Namespace}}.Application.Products.Dtos;

public sealed record ProductDto(
    Guid Id,
    string Name,
    decimal Price,
    string Status,
    DateTime CreatedAtUtc)
{
    public static ProductDto FromEntity(Product product) =>
        new(product.Id, product.Name.Value, product.Price, product.Status.ToString(), product.CreatedAtUtc);
}
