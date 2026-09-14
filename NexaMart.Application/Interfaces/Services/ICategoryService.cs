using NexaMart.Application.Common.Models;
using NexaMart.Application.DTOs.Common;
using NexaMart.Domain.Entities;

namespace NexaMart.Application.Interfaces.Services;

/// <summary>
/// Service contract for complete category catalog management and queries.
/// </summary>
public interface ICategoryService
{
    Task<PagedResult<Category>> GetCategoriesPagedAsync(CategoryFilterDto filter, bool isStaff = false, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Category>> GetAllActiveCategoriesAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Category>> GetAllCategoriesAsync(CancellationToken cancellationToken = default);
    Task<Category?> GetCategoryByIdAsync(int id, bool isStaff = false, CancellationToken cancellationToken = default);
    Task<Category> CreateCategoryAsync(Category category, CancellationToken cancellationToken = default);
    Task<Category> UpdateCategoryAsync(int id, Category category, CancellationToken cancellationToken = default);
    Task<bool> DeleteCategoryAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Category>> GetAllCategoriesWithStatsAsync(CancellationToken cancellationToken = default);
}
