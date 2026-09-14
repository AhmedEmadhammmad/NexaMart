using NexaMart.Domain.Entities;

namespace NexaMart.Application.Interfaces.Repositories;

/// <summary>
/// Unit of Work contract maintaining transactions and coordinating repositories.
/// </summary>
public interface IUnitOfWork : IDisposable, IAsyncDisposable
{
    IGenericRepository<Product> Products { get; }
    IGenericRepository<Category> Categories { get; }
    IGenericRepository<CartItem> CartItems { get; }
    IGenericRepository<WishlistItem> WishlistItems { get; }
    IGenericRepository<Order> Orders { get; }
    IGenericRepository<OrderItem> OrderItems { get; }
    IGenericRepository<Review> Reviews { get; }
    IGenericRepository<ApplicationUser> Users { get; }

    Task<int> CompleteAsync(CancellationToken cancellationToken = default);
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}
