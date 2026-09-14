using Microsoft.EntityFrameworkCore;
using NexaMart.Application.Common.Models;
using NexaMart.Application.DTOs.Common;
using NexaMart.Application.Interfaces.Repositories;
using NexaMart.Application.Interfaces.Services;
using NexaMart.Domain.Entities;

namespace NexaMart.Application.Services;

/// <summary>
/// Service implementing complete product catalog CRUD operations and queries.
/// </summary>
public class ProductService : IProductService
{
    private readonly IUnitOfWork _unitOfWork;

    public ProductService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<PagedResult<Product>> GetProductsPagedAsync(ProductFilterDto filter, bool isStaff = false, CancellationToken cancellationToken = default)
    {
        // Business Rule: Search by ID is restricted to Admin and SuperAdmin
        if (!isStaff)
        {
            filter.SearchId = null;
            filter.IsActiveOnly = true;
        }

        var query = _unitOfWork.Products.Query()
            .Include(p => p.Category)
            .AsQueryable();

        // 1. Exact ID search (Admin & SuperAdmin only)
        if (filter.SearchId.HasValue)
        {
            query = query.Where(p => p.Id == filter.SearchId.Value);
        }

        // 2. Keyword / Name search
        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            var term = filter.SearchTerm.Trim().ToLower();
            query = query.Where(p => p.Name.ToLower().Contains(term) ||
                                     (p.Description != null && p.Description.ToLower().Contains(term)));
        }

        // 3. Category filter
        if (filter.CategoryId.HasValue)
        {
            query = query.Where(p => p.CategoryId == filter.CategoryId.Value);
        }

        // 4. Price range filters
        if (filter.MinPrice.HasValue)
        {
            query = query.Where(p => p.Price >= filter.MinPrice.Value);
        }

        if (filter.MaxPrice.HasValue)
        {
            query = query.Where(p => p.Price <= filter.MaxPrice.Value);
        }

        // 5. Visibility filter
        if (filter.IsActiveOnly == true)
        {
            query = query.Where(p => p.IsActive);
        }

        // 6. Calculate total count before pagination
        var totalCount = await query.CountAsync(cancellationToken);

        // 7. Dynamic multi-criteria sorting
        query = filter.SortBy?.ToLower() switch
        {
            "price_asc" => query.OrderBy(p => p.Price),
            "price_desc" => query.OrderByDescending(p => p.Price),
            "name_asc" => query.OrderBy(p => p.Name),
            "name_desc" => query.OrderByDescending(p => p.Name),
            "top_selling" => query.OrderByDescending(p => p.AverageRating).ThenByDescending(p => p.ReviewCount),
            "newest" => query.OrderByDescending(p => p.Id),
            _ => query.OrderByDescending(p => p.Id)
        };

        // 8. Database-level pagination
        var pageIndex = filter.PageIndex < 1 ? 1 : filter.PageIndex;
        var pageSize = filter.PageSize < 1 ? 10 : filter.PageSize;

        var items = await query
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<Product>(items, totalCount, pageIndex, pageSize);
    }

    public async Task<IReadOnlyList<Product>> GetAllProductsAsync(bool activeOnly = false, CancellationToken cancellationToken = default)
    {
        var query = _unitOfWork.Products.Query()
            .Include(p => p.Category)
            .AsQueryable();

        if (activeOnly)
        {
            query = query.Where(p => p.IsActive);
        }

        return await query
            .OrderByDescending(p => p.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task<Product?> GetProductByIdAsync(int id, bool isStaff = false, CancellationToken cancellationToken = default)
    {
        var product = await _unitOfWork.Products.Query()
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

        if (product == null)
        {
            return null;
        }

        if (!product.IsActive && !isStaff)
        {
            return null;
        }

        return product;
    }

    public async Task<IReadOnlyList<Product>> GetRelatedProductsAsync(int productId, int categoryId, int count = 4, CancellationToken cancellationToken = default)
    {
        return await _unitOfWork.Products.Query()
            .Include(p => p.Category)
            .Where(p => p.CategoryId == categoryId && p.Id != productId && p.IsActive)
            .Take(count)
            .ToListAsync(cancellationToken);
    }

    public async Task<Product> CreateProductAsync(Product product, CancellationToken cancellationToken = default)
    {
        product.CreatedAt = DateTime.UtcNow;
        product.UpdatedAt = null;

        await _unitOfWork.Products.AddAsync(product, cancellationToken);
        await _unitOfWork.CompleteAsync(cancellationToken);

        return (await GetProductByIdAsync(product.Id, isStaff: true, cancellationToken))!;
    }

    public async Task<Product> UpdateProductAsync(int id, Product product, CancellationToken cancellationToken = default)
    {
        var existing = await _unitOfWork.Products.GetByIdAsync(id, cancellationToken);
        if (existing == null)
        {
            throw new KeyNotFoundException($"Product with ID '{id}' was not found.");
        }

        existing.CategoryId = product.CategoryId;
        existing.Name = product.Name;
        existing.Description = product.Description;
        existing.ImageUrl = product.ImageUrl;
        existing.Price = product.Price;
        existing.StockQuantity = product.StockQuantity;
        existing.IsActive = product.IsActive;
        existing.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Products.Update(existing);
        await _unitOfWork.CompleteAsync(cancellationToken);

        return (await GetProductByIdAsync(id, isStaff: true, cancellationToken))!;
    }

    public async Task<bool> DeleteProductAsync(int id, CancellationToken cancellationToken = default)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(id, cancellationToken);
        if (product == null)
        {
            return false;
        }

        _unitOfWork.Products.Delete(product);
        await _unitOfWork.CompleteAsync(cancellationToken);
        return true;
    }

    public async Task<IReadOnlyList<Product>> GetTopSellingProductsAsync(int count = 10, CancellationToken cancellationToken = default)
    {
        return await _unitOfWork.Products.Query()
            .Include(p => p.Category)
            .Where(p => p.IsActive)
            .OrderByDescending(p => p.AverageRating)
            .ThenByDescending(p => p.ReviewCount)
            .Take(count)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _unitOfWork.Products.ExistsAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<(int TotalProducts, int ActiveProducts, int LowStock, decimal AvgRating, int TotalReviews)> GetProductCatalogStatsAsync(CancellationToken cancellationToken = default)
    {
        var stats = await _unitOfWork.Products.Query().AsNoTracking()
            .GroupBy(p => 1)
            .Select(g => new
            {
                TotalProducts = g.Count(),
                ActiveProducts = g.Sum(p => p.IsActive ? 1 : 0),
                LowStock = g.Sum(p => (p.StockQuantity < 10 && p.IsActive) ? 1 : 0),
                AvgRating = g.Average(p => (double)p.AverageRating),
                TotalReviews = g.Sum(p => p.ReviewCount)
            })
            .FirstOrDefaultAsync(cancellationToken);

        return stats != null
            ? (stats.TotalProducts, stats.ActiveProducts, stats.LowStock, Math.Round((decimal)stats.AvgRating, 2), stats.TotalReviews)
            : (0, 0, 0, 0.0m, 0);
    }

    public async Task<IReadOnlyList<Product>> GetRecentProductsAsync(int count = 10, CancellationToken cancellationToken = default)
    {
        return await _unitOfWork.Products.Query()
            .Include(p => p.Category)
            .OrderByDescending(p => p.Id)
            .Take(count)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }
}
