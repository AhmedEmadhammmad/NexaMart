using Microsoft.EntityFrameworkCore;
using NexaMart.Application.Common.Models;
using NexaMart.Application.DTOs.Common;
using NexaMart.Application.Interfaces.Repositories;
using NexaMart.Application.Interfaces.Services;
using NexaMart.Domain.Entities;

namespace NexaMart.Application.Services;

/// <summary>
/// Service implementing complete category catalog CRUD operations and queries.
/// </summary>
public class CategoryService : ICategoryService
{
    private readonly IUnitOfWork _unitOfWork;

    public CategoryService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<PagedResult<Category>> GetCategoriesPagedAsync(CategoryFilterDto filter, bool isStaff = false, CancellationToken cancellationToken = default)
    {
        // Business Rule: Search by ID is restricted to Admin and SuperAdmin
        if (!isStaff)
        {
            filter.SearchId = null;
            filter.IsActiveOnly = true;
        }

        var query = _unitOfWork.Categories.Query()
            .Include(c => c.Products)
            .AsQueryable();

        // 1. Search strictly by ID (Admin & SuperAdmin only)
        if (filter.SearchId.HasValue)
        {
            query = query.Where(c => c.Id == filter.SearchId.Value);
        }

        // 2. Keyword / Name search
        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            var term = filter.SearchTerm.Trim().ToLower();
            query = query.Where(c => c.Name.ToLower().Contains(term) ||
                                     (c.Description != null && c.Description.ToLower().Contains(term)));
        }

        // 3. Visibility filter
        if (filter.IsActiveOnly == true)
        {
            query = query.Where(c => c.IsActive);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var pageIndex = filter.PageIndex < 1 ? 1 : filter.PageIndex;
        var pageSize = filter.PageSize < 1 ? 10 : filter.PageSize;

        var items = await query
            .OrderBy(c => c.DisplayOrder)
            .ThenBy(c => c.Name)
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        foreach (var category in items)
        {
            category.AverageRating = category.Products != null && category.Products.Any()
                ? Math.Round((decimal)category.Products.Average(p => (double)p.AverageRating), 2)
                : 0.0m;
        }

        return new PagedResult<Category>(items, totalCount, pageIndex, pageSize);
    }

    public async Task<IReadOnlyList<Category>> GetAllActiveCategoriesAsync(CancellationToken cancellationToken = default)
    {
        return await _unitOfWork.Categories.Query()
            .Include(c => c.Products)
            .Where(c => c.IsActive)
            .OrderBy(c => c.DisplayOrder)
            .ThenBy(c => c.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Category>> GetAllCategoriesAsync(CancellationToken cancellationToken = default)
    {
        return await _unitOfWork.Categories.Query()
            .Include(c => c.Products)
            .OrderBy(c => c.DisplayOrder)
            .ThenBy(c => c.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<Category?> GetCategoryByIdAsync(int id, bool isStaff = false, CancellationToken cancellationToken = default)
    {
        var category = await _unitOfWork.Categories.Query()
            .Include(c => c.Products)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

        if (category == null)
        {
            return null;
        }

        if (!category.IsActive && !isStaff)
        {
            return null;
        }

        return category;
    }

    public async Task<Category> CreateCategoryAsync(Category category, CancellationToken cancellationToken = default)
    {
        category.CreatedAt = DateTime.UtcNow;
        category.UpdatedAt = null;

        await _unitOfWork.Categories.AddAsync(category, cancellationToken);
        await _unitOfWork.CompleteAsync(cancellationToken);

        return (await GetCategoryByIdAsync(category.Id, isStaff: true, cancellationToken))!;
    }

    public async Task<Category> UpdateCategoryAsync(int id, Category category, CancellationToken cancellationToken = default)
    {
        var existing = await _unitOfWork.Categories.GetByIdAsync(id, cancellationToken);
        if (existing == null)
        {
            throw new KeyNotFoundException($"Category with ID '{id}' was not found.");
        }

        existing.Name = category.Name;
        existing.Description = category.Description;
        existing.ImageUrl = category.ImageUrl;
        existing.DisplayOrder = category.DisplayOrder;
        existing.IsActive = category.IsActive;
        existing.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Categories.Update(existing);
        await _unitOfWork.CompleteAsync(cancellationToken);

        return (await GetCategoryByIdAsync(id, isStaff: true, cancellationToken))!;
    }

    public async Task<bool> DeleteCategoryAsync(int id, CancellationToken cancellationToken = default)
    {
        var category = await _unitOfWork.Categories.GetByIdAsync(id, cancellationToken);
        if (category == null)
        {
            return false;
        }

        // Prevent deletion if category contains associated products
        var hasProducts = await _unitOfWork.Products.ExistsAsync(p => p.CategoryId == id, cancellationToken);
        if (hasProducts)
        {
            throw new InvalidOperationException("Cannot delete category because it contains associated products. Move or delete the products first.");
        }

        _unitOfWork.Categories.Delete(category);
        await _unitOfWork.CompleteAsync(cancellationToken);
        return true;
    }

    public async Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _unitOfWork.Categories.ExistsAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Category>> GetAllCategoriesWithStatsAsync(CancellationToken cancellationToken = default)
    {
        var categories = await _unitOfWork.Categories.Query()
            .Include(c => c.Products)
            .OrderBy(c => c.DisplayOrder)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        foreach (var category in categories)
        {
            category.AverageRating = category.Products != null && category.Products.Any()
                ? Math.Round((decimal)category.Products.Average(p => (double)p.AverageRating), 2)
                : 0.0m;
        }

        return categories;
    }
}
