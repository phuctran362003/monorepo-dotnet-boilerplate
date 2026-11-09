using {{Namespace}}.Models;

namespace {{Namespace}}.ViewModels;

public sealed record ProductViewModel(
    Guid Id,
    string Name,
    decimal Price,
    string Status,
    DateTime CreatedAt)
{
    public static ProductViewModel FromEntity(Product product) =>
        new(product.Id, product.Name, product.Price, product.Status, product.CreatedAt);
}
