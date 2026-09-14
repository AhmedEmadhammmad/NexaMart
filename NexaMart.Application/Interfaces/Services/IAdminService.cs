using NexaMart.Application.Common.Models;
using NexaMart.Application.DTOs.Admin;
using NexaMart.Application.DTOs.Common;
using NexaMart.Domain.Entities;
using NexaMart.Domain.Enums;

namespace NexaMart.Application.Interfaces.Services;

/// <summary>
/// Dedicated Application Service contract for Store Admin operations.
/// Encapsulates all dashboard aggregation, universal ID search resolution,
/// catalog CRUD workflows, and order status updates.
/// </summary>
public interface IAdminService
{
    // Dashboard & Universal Search
    Task<AdminDashboardDto> GetDashboardAsync(CancellationToken cancellationToken = default);
    Task<(string RouteAction, string RouteController, object? RouteValues, string? ErrorMessage)> ResolveSearchByIdAsync(int id, string? type = null, CancellationToken cancellationToken = default);

    // Products Management
    Task<PagedResult<Product>> GetProductsPagedAsync(ProductFilterDto filter, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Category>> GetActiveCategoriesAsync(CancellationToken cancellationToken = default);
    Task<Product?> GetProductByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Product> CreateProductAsync(Product product, CancellationToken cancellationToken = default);
    Task<Product> UpdateProductAsync(int id, Product product, CancellationToken cancellationToken = default);
    Task<bool> ToggleProductStatusAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> DeleteProductAsync(int id, CancellationToken cancellationToken = default);

    // Categories Management
    Task<PagedResult<Category>> GetCategoriesPagedAsync(CategoryFilterDto filter, CancellationToken cancellationToken = default);
    Task<Category?> GetCategoryByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Category> CreateCategoryAsync(Category category, CancellationToken cancellationToken = default);
    Task<Category> UpdateCategoryAsync(int id, Category category, CancellationToken cancellationToken = default);
    Task<bool> ToggleCategoryStatusAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> DeleteCategoryAsync(int id, CancellationToken cancellationToken = default);

    // Orders Management
    Task<PagedResult<Order>> GetOrdersPagedAsync(OrderStatus? status, int? searchId, string? searchTerm, int pageIndex = 1, int pageSize = 15, CancellationToken cancellationToken = default);
    Task<bool> UpdateOrderStatusAsync(int orderId, OrderStatus status, CancellationToken cancellationToken = default);
}
