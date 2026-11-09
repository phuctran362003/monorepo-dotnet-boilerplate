using {{ProductEntityNamespace}};
using {{RepositoryInterfaceNamespace}};

namespace {{UnitOfWorkInterfaceNamespace}};

public interface IUnitOfWork : IDisposable
{
    IGenericRepository<Product> Products { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    Task BeginTransactionAsync(CancellationToken cancellationToken = default);

    Task CommitAsync(CancellationToken cancellationToken = default);

    Task RollbackAsync(CancellationToken cancellationToken = default);
}
