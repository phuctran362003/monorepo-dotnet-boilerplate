using {{Namespace}}.ViewModels;

namespace {{Namespace}}.Services;

/// <summary>Application service exposing the product use cases.</summary>
public interface IProductService
{
    Task<IReadOnlyList<ProductViewModel>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<ProductViewModel?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<ProductViewModel> CreateAsync(
        CreateProductViewModel request,
        CancellationToken cancellationToken = default);
}
