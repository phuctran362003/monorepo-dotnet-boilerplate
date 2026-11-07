using {{Namespace}}.Application.Products.Dtos;

namespace {{Namespace}}.Application.Products;

/// <summary>Application service exposing the product use cases.</summary>
public interface IProductService
{
    Task<IReadOnlyList<ProductDto>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<ProductDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<ProductDto> CreateAsync(CreateProductRequest request, CancellationToken cancellationToken = default);
}
