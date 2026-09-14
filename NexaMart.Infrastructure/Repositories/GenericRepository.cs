using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using NexaMart.Application.Interfaces.Repositories;
using NexaMart.Infrastructure.Data.Context;

namespace NexaMart.Infrastructure.Repositories;

/// <summary>
/// Generic repository implementation utilizing Entity Framework Core.
/// </summary>
/// <typeparam name="T">Entity class type</typeparam>
public class GenericRepository<T> : IGenericRepository<T> where T : class
{
    protected readonly NexaMartDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public GenericRepository(NexaMartDbContext context)
    {
        _context = context;
        _dbSet = _context.Set<T>();
    }

    public virtual IQueryable<T> Query(bool disableTracking = true)
    {
        return disableTracking ? _dbSet.AsNoTracking() : _dbSet;
    }

    public virtual async Task<T?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FindAsync(new object[] { id }, cancellationToken);
    }

    public virtual async Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet.AsNoTracking().ToListAsync(cancellationToken);
    }

    public virtual async Task<IReadOnlyList<T>> GetAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AsNoTracking().Where(predicate).ToListAsync(cancellationToken);
    }

    public virtual async Task<IReadOnlyList<T>> GetAsync(
        Expression<Func<T, bool>>? predicate = null,
        Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
        string? includeProperties = null,
        bool disableTracking = true,
        CancellationToken cancellationToken = default)
    {
        IQueryable<T> query = _dbSet;

        if (disableTracking)
        {
            query = query.AsNoTracking();
        }

        if (predicate != null)
        {
            query = query.Where(predicate);
        }

        if (!string.IsNullOrWhiteSpace(includeProperties))
        {
            foreach (var includeProperty in includeProperties.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                query = query.Include(includeProperty.Trim());
            }
        }

        if (orderBy != null)
        {
            return await orderBy(query).ToListAsync(cancellationToken);
        }

        return await query.ToListAsync(cancellationToken);
    }

    public virtual async Task<T?> FirstOrDefaultAsync(
        Expression<Func<T, bool>> predicate,
        string? includeProperties = null,
        bool disableTracking = true,
        CancellationToken cancellationToken = default)
    {
        IQueryable<T> query = _dbSet;

        if (disableTracking)
        {
            query = query.AsNoTracking();
        }

        if (!string.IsNullOrWhiteSpace(includeProperties))
        {
            foreach (var includeProperty in includeProperties.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                query = query.Include(includeProperty.Trim());
            }
        }

        return await query.FirstOrDefaultAsync(predicate, cancellationToken);
    }

    public virtual async Task AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        await _dbSet.AddAsync(entity, cancellationToken);
    }

    public virtual async Task AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
    {
        await _dbSet.AddRangeAsync(entities, cancellationToken);
    }

    public virtual void Update(T entity)
    {
        var entry = _context.Entry(entity);
        if (entry.State == EntityState.Detached)
        {
            _dbSet.Attach(entity);
        }
        entry.State = EntityState.Modified;
    }

    public virtual void Delete(T entity)
    {
        if (_context.Entry(entity).State == EntityState.Detached)
        {
            _dbSet.Attach(entity);
        }
        _dbSet.Remove(entity);
    }

    public virtual void DeleteRange(IEnumerable<T> entities)
    {
        _dbSet.RemoveRange(entities);
    }

    public virtual async Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null, CancellationToken cancellationToken = default)
    {
        if (predicate == null)
        {
            return await _dbSet.CountAsync(cancellationToken);
        }

        return await _dbSet.CountAsync(predicate, cancellationToken);
    }

    public virtual async Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AnyAsync(predicate, cancellationToken);
    }
}
