using System.Linq.Expressions;
using {{DomainNamespace}};
using {{DomainNamespace}}.Entities;
using Microsoft.EntityFrameworkCore;

public class GenericRepository<TEntity> : IGenericRepository<TEntity> where TEntity : BaseEntity
{
    private readonly DbSet<TEntity> _dbSet;

    public GenericRepository({{SolutionName}}DbContext context)
    {
        _dbSet = context.Set<TEntity>();
    }

    public async Task<List<TEntity>> GetAllAsync(
        Expression<Func<TEntity, bool>>? predicate = null,
        params Expression<Func<TEntity, object>>[] includes)
    {
        var query = ApplyIncludes(_dbSet.AsQueryable(), includes);
        if (predicate is not null)
        {
            query = query.Where(predicate);
        }

        return await query.ToListAsync();
    }

    public async Task<TEntity?> GetByIdAsync(Guid id, params Expression<Func<TEntity, object>>[] includes)
    {
        var query = ApplyIncludes(_dbSet.AsQueryable(), includes);
        return await query.FirstOrDefaultAsync(entity => entity.Id == id);
    }

    public async Task<TEntity?> FirstOrDefaultAsync(
        Expression<Func<TEntity, bool>>? predicate = null,
        params Expression<Func<TEntity, object>>[] includes)
    {
        var query = ApplyIncludes(_dbSet.AsQueryable(), includes);
        if (predicate is not null)
        {
            query = query.Where(predicate);
        }

        return await query.FirstOrDefaultAsync();
    }

    public IQueryable<TEntity> GetQueryable() => _dbSet.AsQueryable();

    public Task<int> CountAsync(Expression<Func<TEntity, bool>>? predicate = null) =>
        predicate is null ? _dbSet.CountAsync() : _dbSet.CountAsync(predicate);

    public async Task AddAsync(TEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        var now = DateTime.UtcNow;
        if (entity.Id == Guid.Empty)
        {
            entity.Id = Guid.NewGuid();
        }

        entity.CreatedAt = now;
        entity.UpdatedAt = now;
        await _dbSet.AddAsync(entity);
    }

    public async Task AddRangeAsync(List<TEntity> entities)
    {
        ArgumentNullException.ThrowIfNull(entities);

        var now = DateTime.UtcNow;
        foreach (var entity in entities)
        {
            if (entity.Id == Guid.Empty)
            {
                entity.Id = Guid.NewGuid();
            }

            entity.CreatedAt = now;
            entity.UpdatedAt = now;
        }

        await _dbSet.AddRangeAsync(entities);
    }

    public void Update(TEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        entity.UpdatedAt = DateTime.UtcNow;
        _dbSet.Update(entity);
    }

    public void UpdateRange(List<TEntity> entities)
    {
        ArgumentNullException.ThrowIfNull(entities);

        foreach (var entity in entities)
        {
            entity.UpdatedAt = DateTime.UtcNow;
        }

        _dbSet.UpdateRange(entities);
    }

    public void HardRemove(TEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        _dbSet.Remove(entity);
    }

    public void HardRemoveRange(List<TEntity> entities)
    {
        ArgumentNullException.ThrowIfNull(entities);
        _dbSet.RemoveRange(entities);
    }

    public void SoftRemove(TEntity entity) =>
        throw new NotSupportedException("Soft delete is not supported.");

    public void SoftRemoveRange(List<TEntity> entities) =>
        throw new NotSupportedException("Soft delete is not supported.");

    private static IQueryable<TEntity> ApplyIncludes(
        IQueryable<TEntity> query,
        params Expression<Func<TEntity, object>>[] includes)
    {
        foreach (var include in includes)
        {
            query = query.Include(include);
        }

        return query;
    }
}
