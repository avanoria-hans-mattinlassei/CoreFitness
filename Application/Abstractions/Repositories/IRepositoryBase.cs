namespace Application.Abstractions.Repositories;

public interface IRepositoryBase<TModel, TId>
{
    Task AddAsync(TModel model, CancellationToken ct = default);
    Task<bool> ExistsAsync(TId id, CancellationToken ct = default);
    Task<TModel?> GetByIdAsync(TId id, CancellationToken ct = default);
    Task<IReadOnlyList<TModel>> GetPageAsync(int page, int pageSize, CancellationToken ct = default);
    Task<bool> RemoveAsync(TId id, CancellationToken ct = default);
    Task<bool> UpdateAsync(TModel model, CancellationToken ct = default);
}
