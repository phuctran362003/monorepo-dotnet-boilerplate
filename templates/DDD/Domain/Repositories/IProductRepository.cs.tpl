using {{DomainNamespace}}.Entities;

namespace {{DomainNamespace}}.Repositories;

/// <summary>Repository contract for the <see cref="Product"/> aggregate.</summary>
public interface IProductRepository
{
    Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Product>> GetAllAsync(CancellationToken cancellationToken = default);

    Task AddAsync(Product product, CancellationToken cancellationToken = default);

    void Update(Product product);

    void Remove(Product product);
}
