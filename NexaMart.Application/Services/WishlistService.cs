using Microsoft.EntityFrameworkCore;
using NexaMart.Application.Interfaces.Repositories;
using NexaMart.Application.Interfaces.Services;
using NexaMart.Domain.Entities;

namespace NexaMart.Application.Services;

/// <summary>
/// Service managing customer wishlist items, toggle actions, and moving items to cart.
/// </summary>
public class WishlistService : IWishlistService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICartService _cartService;

    public WishlistService(IUnitOfWork unitOfWork, ICartService cartService)
    {
        _unitOfWork = unitOfWork;
        _cartService = cartService;
    }

    public async Task<IReadOnlyList<WishlistItem>> GetUserWishlistAsync(int userId, CancellationToken cancellationToken = default)
    {
        return await _unitOfWork.WishlistItems.Query()
            .Include(w => w.Product)
                .ThenInclude(p => p.Category)
            .Where(w => w.UserId == userId)
            .OrderByDescending(w => w.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ToggleWishlistAsync(int userId, int productId, CancellationToken cancellationToken = default)
    {
        var existing = await _unitOfWork.WishlistItems.Query()
            .FirstOrDefaultAsync(w => w.UserId == userId && w.ProductId == productId, cancellationToken);

        if (existing != null)
        {
            _unitOfWork.WishlistItems.Delete(existing);
            await _unitOfWork.CompleteAsync(cancellationToken);
            return false; // Removed
        }

        var product = await _unitOfWork.Products.GetByIdAsync(productId, cancellationToken);
        if (product == null || !product.IsActive)
        {
            throw new KeyNotFoundException($"Product with ID {productId} was not found or is inactive.");
        }

        var wishlistItem = new WishlistItem
        {
            UserId = userId,
            ProductId = productId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = null
        };

        await _unitOfWork.WishlistItems.AddAsync(wishlistItem, cancellationToken);
        await _unitOfWork.CompleteAsync(cancellationToken);
        return true; // Added
    }

    public async Task<bool> IsInWishlistAsync(int userId, int productId, CancellationToken cancellationToken = default)
    {
        return await _unitOfWork.WishlistItems.Query()
            .AnyAsync(w => w.UserId == userId && w.ProductId == productId, cancellationToken);
    }

    public async Task<int> GetWishlistCountAsync(int userId, CancellationToken cancellationToken = default)
    {
        return await _unitOfWork.WishlistItems.Query()
            .CountAsync(w => w.UserId == userId, cancellationToken);
    }

    public async Task<bool> RemoveFromWishlistAsync(int userId, int productId, CancellationToken cancellationToken = default)
    {
        var item = await _unitOfWork.WishlistItems.Query()
            .FirstOrDefaultAsync(w => w.UserId == userId && w.ProductId == productId, cancellationToken);

        if (item == null)
        {
            return false;
        }

        _unitOfWork.WishlistItems.Delete(item);
        await _unitOfWork.CompleteAsync(cancellationToken);
        return true;
    }

    public async Task<bool> MoveToCartAsync(int userId, int productId, CancellationToken cancellationToken = default)
    {
        await _cartService.AddToCartAsync(userId, productId, 1, cancellationToken);
        await RemoveFromWishlistAsync(userId, productId, cancellationToken);
        return true;
    }

    public async Task<bool> ClearWishlistAsync(int userId, CancellationToken cancellationToken = default)
    {
        var items = await _unitOfWork.WishlistItems.GetAsync(w => w.UserId == userId, cancellationToken);
        if (items.Count == 0)
        {
            return true;
        }

        _unitOfWork.WishlistItems.DeleteRange(items);
        await _unitOfWork.CompleteAsync(cancellationToken);
        return true;
    }
}
