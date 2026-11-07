namespace {{ApplicationNamespace}}.Common.Interfaces;

/// <summary>Commits pending changes tracked by the persistence layer.</summary>
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
