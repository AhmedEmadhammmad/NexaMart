using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using NexaMart.Application.Interfaces.Repositories;
using NexaMart.Domain.Entities;
using NexaMart.Infrastructure.Data.Context;

namespace NexaMart.Infrastructure.Repositories;

/// <summary>
/// Unit of work implementation coordinating atomic transactions and repository operations.
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly NexaMartDbContext _context;
    private IDbContextTransaction? _transaction;

    public UnitOfWork(NexaMartDbContext context)
    {
        _context = context;
        Products = new GenericRepository<Product>(_context);
        Categories = new GenericRepository<Category>(_context);
        CartItems = new GenericRepository<CartItem>(_context);
        WishlistItems = new GenericRepository<WishlistItem>(_context);
        Orders = new GenericRepository<Order>(_context);
        OrderItems = new GenericRepository<OrderItem>(_context);
        Reviews = new GenericRepository<Review>(_context);
        Users = new GenericRepository<ApplicationUser>(_context);
    }

    public IGenericRepository<Product> Products { get; }
    public IGenericRepository<Category> Categories { get; }
    public IGenericRepository<CartItem> CartItems { get; }
    public IGenericRepository<WishlistItem> WishlistItems { get; }
    public IGenericRepository<Order> Orders { get; }
    public IGenericRepository<OrderItem> OrderItems { get; }
    public IGenericRepository<Review> Reviews { get; }
    public IGenericRepository<ApplicationUser> Users { get; }

    public async Task<int> CompleteAsync(CancellationToken cancellationToken = default)
    {
        UpdateTimestamps();
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        _transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            UpdateTimestamps();
            await _context.SaveChangesAsync(cancellationToken);
            if (_transaction != null)
            {
                await _transaction.CommitAsync(cancellationToken);
            }
        }
        catch
        {
            await RollbackTransactionAsync(cancellationToken);
            throw;
        }
        finally
        {
            if (_transaction != null)
            {
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync(cancellationToken);
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
        GC.SuppressFinalize(this);
    }

    public async ValueTask DisposeAsync()
    {
        if (_transaction != null)
        {
            await _transaction.DisposeAsync();
        }
        await _context.DisposeAsync();
        GC.SuppressFinalize(this);
    }

    private void UpdateTimestamps()
    {
        foreach (var entry in _context.ChangeTracker.Entries())
        {
            if (entry.State == EntityState.Added)
            {
                var createdAtProp = entry.Metadata.FindProperty("CreatedAt");
                if (createdAtProp != null)
                {
                    var currentVal = entry.Property("CreatedAt").CurrentValue;
                    if (currentVal == null || (currentVal is DateTime dt && dt == default))
                    {
                        entry.Property("CreatedAt").CurrentValue = DateTime.UtcNow;
                    }
                }
            }
            else if (entry.State == EntityState.Modified)
            {
                var updatedAtProp = entry.Metadata.FindProperty("UpdatedAt");
                if (updatedAtProp != null)
                {
                    entry.Property("UpdatedAt").CurrentValue = DateTime.UtcNow;
                }
            }
        }
    }
}
