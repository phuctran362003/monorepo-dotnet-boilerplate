using System.Linq.Expressions;
using {{BaseEntityNamespace}};

namespace {{RepositoryInterfaceNamespace}};

public interface IGenericRepository<TEntity> where TEntity : BaseEntity
{
    Task<List<TEntity>> GetAllAsync(
        Expression<Func<TEntity, bool>>? predicate = null,
        params Expression<Func<TEntity, object>>[] includes);

    Task<TEntity?> GetByIdAsync(Guid id, params Expression<Func<TEntity, object>>[] includes);

    Task<TEntity> AddAsync(TEntity entity);

    Task AddRangeAsync(List<TEntity> entities);

    Task<bool> Update(TEntity entity);

    Task<bool> UpdateRange(List<TEntity> entities);

    Task<bool> SoftRemove(TEntity entity);

    Task<bool> SoftRemoveRange(List<TEntity> entities);

    Task<bool> SoftRemoveRangeById(List<Guid> entitiesId);

    Task<bool> HardRemove(Expression<Func<TEntity, bool>> predicate);

    Task<bool> HardRemoveRange(List<TEntity> entities);

    Task<TEntity?> FirstOrDefaultAsync(
        Expression<Func<TEntity, bool>>? predicate = null,
        params Expression<Func<TEntity, object>>[] includes);

    Task<int> CountAsync(Expression<Func<TEntity, bool>> predicate);

    IQueryable<TEntity> GetQueryable();
}
