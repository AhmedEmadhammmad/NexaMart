using NexaMart.Domain.Entities;

namespace NexaMart.Application.Interfaces.Services;

/// <summary>
/// Service contract for complete shopping cart management and operations.
/// </summary>
public interface ICartService
{
    Task<IReadOnlyList<CartItem>> GetCartAsync(int userId, CancellationToken cancellationToken = default);
    Task<CartItem?> GetCartItemByIdAsync(int cartItemId, CancellationToken cancellationToken = default);
    Task<CartItem?> GetCartItemAsync(int userId, int productId, CancellationToken cancellationToken = default);
    Task<CartItem> AddToCartAsync(int userId, int productId, int quantity = 1, CancellationToken cancellationToken = default);
    Task<bool> UpdateQuantityAsync(int userId, int cartItemId, int quantity, CancellationToken cancellationToken = default);
    Task<bool> RemoveFromCartAsync(int userId, int cartItemId, CancellationToken cancellationToken = default);
    Task<bool> ClearCartAsync(int userId, CancellationToken cancellationToken = default);
    Task<int> GetCartCountAsync(int userId, CancellationToken cancellationToken = default);
    Task<decimal> GetCartTotalAsync(int userId, CancellationToken cancellationToken = default);
}
