using Application.Abstractions.Repositories;
using Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Infrastructure.Persistence.Repositories;

public abstract class RepositoryBase<TModel, TId, TEntity>(PersistenceContext context) : IRepositoryBase<TModel, TId> where TEntity : class
{
    protected PersistenceContext Context { get; } = context;
    protected DbSet<TEntity> Entities { get; } = context.Set<TEntity>();

    protected abstract TId GetModelId(TModel model);
    protected abstract TEntity ToEntity(TModel model);
    protected abstract void HandlePropertyUpdates(TEntity entity, TModel model);
    protected abstract Expression<Func<TEntity, bool>> BuildIdPredicate(TId id);
    protected abstract Expression<Func<TEntity, TModel>> ProjectToModel();
    protected abstract IQueryable<TEntity> ApplyDefaultOrdering(IQueryable<TEntity> query);

    public virtual async Task AddAsync(TModel model, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(model);

        var entity = ToEntity(model);
        await Entities.AddAsync(entity, ct);
    }

    public virtual async Task<bool> UpdateAsync(TModel model, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(model);

        var id = GetModelId(model);
        var entity = await Entities.FirstOrDefaultAsync(BuildIdPredicate(id), ct);

        if (entity is null)
            return false;

        HandlePropertyUpdates(entity, model);
        return true;
    }

    public virtual async Task<bool> RemoveAsync(TId id, CancellationToken ct = default)
    {
        var entity = await Entities.FirstOrDefaultAsync(BuildIdPredicate(id), ct);

        if (entity is null)
            return false;

        Entities.Remove(entity);
        return true;
    }

    public virtual Task<bool> ExistsAsync(TId id, CancellationToken ct = default)
    {
        return Entities
            .AsNoTracking()
            .AnyAsync(BuildIdPredicate(id), ct);
    }

    public virtual Task<TModel?> GetByIdAsync(TId id, CancellationToken ct = default)
    {
        return Entities
            .AsNoTracking()
            .Where(BuildIdPredicate(id))
            .Select(ProjectToModel())
            .FirstOrDefaultAsync(ct);
    }

    public virtual async Task<IReadOnlyList<TModel>> GetPageAsync(int page, int pageSize, CancellationToken ct = default)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(page, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(pageSize, 1);

        return await ApplyDefaultOrdering(Entities.AsNoTracking())
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(ProjectToModel())
            .ToListAsync(ct);
    }
}