using System.Linq.Expressions;
using {{DomainNamespace}}.Entities;

public interface IGenericRepository<TEntity> where TEntity : BaseEntity
{
    Task<List<TEntity>> GetAllAsync(
        Expression<Func<TEntity, bool>>? predicate = null,
        params Expression<Func<TEntity, object>>[] includes);

    Task<TEntity?> GetByIdAsync(Guid id, params Expression<Func<TEntity, object>>[] includes);

    Task<TEntity?> FirstOrDefaultAsync(
        Expression<Func<TEntity, bool>>? predicate = null,
        params Expression<Func<TEntity, object>>[] includes);

    IQueryable<TEntity> GetQueryable();

    Task<int> CountAsync(Expression<Func<TEntity, bool>>? predicate = null);

    Task AddAsync(TEntity entity);

    Task AddRangeAsync(List<TEntity> entities);

    void Update(TEntity entity);

    void UpdateRange(List<TEntity> entities);

    void HardRemove(TEntity entity);

    void HardRemoveRange(List<TEntity> entities);

    void SoftRemove(TEntity entity);

    void SoftRemoveRange(List<TEntity> entities);
}
