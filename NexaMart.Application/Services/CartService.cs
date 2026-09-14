using Microsoft.EntityFrameworkCore;
using NexaMart.Application.Interfaces.Repositories;
using NexaMart.Application.Interfaces.Services;
using NexaMart.Domain.Entities;

namespace NexaMart.Application.Services;

/// <summary>
/// Service implementing complete shopping cart CRUD operations, stock validation, and totals calculation.
/// </summary>
public class CartService : ICartService
{
    private readonly IUnitOfWork _unitOfWork;

    public CartService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<CartItem>> GetCartAsync(int userId, CancellationToken cancellationToken = default)
    {
        return await _unitOfWork.CartItems.Query()
            .Include(c => c.Product)
                .ThenInclude(p => p.Category)
            .Where(c => c.UserId == userId)
            .OrderByDescending(c => c.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task<CartItem?> GetCartItemByIdAsync(int cartItemId, CancellationToken cancellationToken = default)
    {
        return await _unitOfWork.CartItems.Query()
            .Include(c => c.Product)
                .ThenInclude(p => p.Category)
            .FirstOrDefaultAsync(c => c.Id == cartItemId, cancellationToken);
    }

    public async Task<CartItem?> GetCartItemAsync(int userId, int productId, CancellationToken cancellationToken = default)
    {
        return await _unitOfWork.CartItems.Query()
            .Include(c => c.Product)
                .ThenInclude(p => p.Category)
            .FirstOrDefaultAsync(c => c.UserId == userId && c.ProductId == productId, cancellationToken);
    }

    public async Task<CartItem> AddToCartAsync(int userId, int productId, int quantity = 1, CancellationToken cancellationToken = default)
    {
        if (quantity <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be greater than zero.");
        }

        var product = await _unitOfWork.Products.GetByIdAsync(productId, cancellationToken);
        if (product == null || !product.IsActive)
        {
            throw new KeyNotFoundException($"Product with ID {productId} was not found or is inactive.");
        }

        if (product.StockQuantity < quantity)
        {
            throw new InvalidOperationException($"Insufficient inventory. Only {product.StockQuantity} available.");
        }

        var existingCartItem = await _unitOfWork.CartItems.Query()
            .FirstOrDefaultAsync(c => c.UserId == userId && c.ProductId == productId, cancellationToken);

        if (existingCartItem != null)
        {
            var newQuantity = existingCartItem.Quantity + quantity;
            if (newQuantity > product.StockQuantity)
            {
                throw new InvalidOperationException($"Cannot add more than available inventory ({product.StockQuantity}).");
            }

            existingCartItem.Quantity = newQuantity;
            existingCartItem.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.CartItems.Update(existingCartItem);
            await _unitOfWork.CompleteAsync(cancellationToken);

            return (await GetCartItemByIdAsync(existingCartItem.Id, cancellationToken))!;
        }

        var cartItem = new CartItem
        {
            UserId = userId,
            ProductId = productId,
            Quantity = quantity,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = null
        };

        await _unitOfWork.CartItems.AddAsync(cartItem, cancellationToken);
        await _unitOfWork.CompleteAsync(cancellationToken);

        return (await GetCartItemByIdAsync(cartItem.Id, cancellationToken))!;
    }

    public async Task<bool> UpdateQuantityAsync(int userId, int cartItemId, int quantity, CancellationToken cancellationToken = default)
    {
        var item = await _unitOfWork.CartItems.Query()
            .Include(c => c.Product)
            .FirstOrDefaultAsync(c => c.Id == cartItemId && c.UserId == userId, cancellationToken);

        if (item == null)
        {
            return false;
        }

        if (quantity <= 0)
        {
            _unitOfWork.CartItems.Delete(item);
            await _unitOfWork.CompleteAsync(cancellationToken);
            return true;
        }

        if (quantity > item.Product.StockQuantity)
        {
            throw new InvalidOperationException($"Only {item.Product.StockQuantity} items currently in stock.");
        }

        item.Quantity = quantity;
        item.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.CartItems.Update(item);
        await _unitOfWork.CompleteAsync(cancellationToken);
        return true;
    }

    public async Task<bool> RemoveFromCartAsync(int userId, int cartItemId, CancellationToken cancellationToken = default)
    {
        var item = await _unitOfWork.CartItems.Query()
            .FirstOrDefaultAsync(c => c.Id == cartItemId && c.UserId == userId, cancellationToken);

        if (item == null)
        {
            return false;
        }

        _unitOfWork.CartItems.Delete(item);
        await _unitOfWork.CompleteAsync(cancellationToken);
        return true;
    }

    public async Task<bool> ClearCartAsync(int userId, CancellationToken cancellationToken = default)
    {
        var items = await _unitOfWork.CartItems.GetAsync(c => c.UserId == userId, cancellationToken);
        if (items.Count == 0)
        {
            return true;
        }

        _unitOfWork.CartItems.DeleteRange(items);
        await _unitOfWork.CompleteAsync(cancellationToken);
        return true;
    }

    public async Task<int> GetCartCountAsync(int userId, CancellationToken cancellationToken = default)
    {
        return await _unitOfWork.CartItems.Query()
            .Where(c => c.UserId == userId)
            .SumAsync(c => c.Quantity, cancellationToken);
    }

    public async Task<decimal> GetCartTotalAsync(int userId, CancellationToken cancellationToken = default)
    {
        return await _unitOfWork.CartItems.Query()
            .Include(c => c.Product)
            .Where(c => c.UserId == userId)
            .SumAsync(c => c.Quantity * c.Product.Price, cancellationToken);
    }
}
