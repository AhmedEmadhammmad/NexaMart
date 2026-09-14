using System.Linq.Expressions;

namespace NexaMart.Application.Interfaces.Repositories;

/// <summary>
/// Generic repository contract defining common asynchronous data persistence and querying operations.
/// </summary>
/// <typeparam name="T">Entity class type</typeparam>
public interface IGenericRepository<T> where T : class
{
    IQueryable<T> Query(bool disableTracking = true);

    Task<T?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<T>> GetAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
    
    Task<IReadOnlyList<T>> GetAsync(
        Expression<Func<T, bool>>? predicate = null,
        Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
        string? includeProperties = null,
        bool disableTracking = true,
        CancellationToken cancellationToken = default);

    Task<T?> FirstOrDefaultAsync(
        Expression<Func<T, bool>> predicate,
        string? includeProperties = null,
        bool disableTracking = true,
        CancellationToken cancellationToken = default);

    Task AddAsync(T entity, CancellationToken cancellationToken = default);
    Task AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);
    
    void Update(T entity);
    void Delete(T entity);
    void DeleteRange(IEnumerable<T> entities);

    Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
}
