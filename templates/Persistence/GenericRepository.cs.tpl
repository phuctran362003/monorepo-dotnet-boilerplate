using System.Linq.Expressions;
using {{BaseEntityNamespace}};
using {{DbContextNamespace}};
using {{RepositoryInterfaceNamespace}};
using Microsoft.EntityFrameworkCore;

namespace {{RepositoryImplementationNamespace}};

public class GenericRepository<TEntity> : IGenericRepository<TEntity> where TEntity : BaseEntity
{
    private readonly DbSet<TEntity> _dbSet;

    public GenericRepository(ApplicationDbContext context)
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

        return await query.Where(entity => !entity.IsDeleted).ToListAsync();
    }

    public async Task<TEntity?> GetByIdAsync(Guid id, params Expression<Func<TEntity, object>>[] includes)
    {
        var query = ApplyIncludes(_dbSet.AsQueryable(), includes);
        return await query.FirstOrDefaultAsync(entity => entity.Id == id && !entity.IsDeleted);
    }

    public async Task<TEntity> AddAsync(TEntity entity)
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
        return entity;
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

    public Task<bool> Update(TEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        entity.UpdatedAt = DateTime.UtcNow;
        _dbSet.Update(entity);
        return Task.FromResult(true);
    }

    public Task<bool> UpdateRange(List<TEntity> entities)
    {
        ArgumentNullException.ThrowIfNull(entities);

        foreach (var entity in entities)
        {
            entity.UpdatedAt = DateTime.UtcNow;
        }

        _dbSet.UpdateRange(entities);
        return Task.FromResult(true);
    }

    public Task<bool> SoftRemove(TEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        var now = DateTime.UtcNow;
        entity.IsDeleted = true;
        entity.DeletedAt = now;
        entity.UpdatedAt = now;
        _dbSet.Update(entity);
        return Task.FromResult(true);
    }

    public async Task<bool> SoftRemoveRange(List<TEntity> entities)
    {
        ArgumentNullException.ThrowIfNull(entities);

        foreach (var entity in entities)
        {
            await SoftRemove(entity);
        }

        return true;
    }

    public async Task<bool> SoftRemoveRangeById(List<Guid> entitiesId)
    {
        ArgumentNullException.ThrowIfNull(entitiesId);

        var entities = await _dbSet.Where(entity => entitiesId.Contains(entity.Id) && !entity.IsDeleted).ToListAsync();
        return await SoftRemoveRange(entities);
    }

    public async Task<bool> HardRemove(Expression<Func<TEntity, bool>> predicate)
    {
        ArgumentNullException.ThrowIfNull(predicate);

        var entities = await _dbSet.Where(predicate).ToListAsync();
        _dbSet.RemoveRange(entities);
        return true;
    }

    public Task<bool> HardRemoveRange(List<TEntity> entities)
    {
        ArgumentNullException.ThrowIfNull(entities);

        _dbSet.RemoveRange(entities);
        return Task.FromResult(true);
    }

    public async Task<TEntity?> FirstOrDefaultAsync(
        Expression<Func<TEntity, bool>>? predicate = null,
        params Expression<Func<TEntity, object>>[] includes)
    {
        var query = ApplyIncludes(_dbSet.AsQueryable(), includes).Where(entity => !entity.IsDeleted);
        if (predicate is not null)
        {
            query = query.Where(predicate);
        }

        return await query.FirstOrDefaultAsync();
    }

    public Task<int> CountAsync(Expression<Func<TEntity, bool>> predicate)
    {
        ArgumentNullException.ThrowIfNull(predicate);

        return _dbSet.Where(entity => !entity.IsDeleted).Where(predicate).CountAsync();
    }

    public IQueryable<TEntity> GetQueryable() => _dbSet.AsQueryable().Where(entity => !entity.IsDeleted);

    private static IQueryable<TEntity> ApplyIncludes(
        IQueryable<TEntity> query,
        params Expression<Func<TEntity, object>>[] includes)
    {
        if (includes is null)
        {
            return query;
        }

        foreach (var include in includes)
        {
            query = query.Include(include);
        }

        return query;
    }
}
