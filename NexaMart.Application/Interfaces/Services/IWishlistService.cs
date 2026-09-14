using NexaMart.Domain.Entities;

namespace NexaMart.Application.Interfaces.Services;

/// <summary>
/// Wishlist service contract handling customer wishlist items and toggle operations.
/// </summary>
public interface IWishlistService
{
    Task<IReadOnlyList<WishlistItem>> GetUserWishlistAsync(int userId, CancellationToken cancellationToken = default);
    Task<bool> ToggleWishlistAsync(int userId, int productId, CancellationToken cancellationToken = default);
    Task<bool> IsInWishlistAsync(int userId, int productId, CancellationToken cancellationToken = default);
    Task<int> GetWishlistCountAsync(int userId, CancellationToken cancellationToken = default);
    Task<bool> RemoveFromWishlistAsync(int userId, int productId, CancellationToken cancellationToken = default);
    Task<bool> MoveToCartAsync(int userId, int productId, CancellationToken cancellationToken = default);
    Task<bool> ClearWishlistAsync(int userId, CancellationToken cancellationToken = default);
}
