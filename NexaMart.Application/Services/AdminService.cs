using NexaMart.Application.Common.Models;
using NexaMart.Application.DTOs.Admin;
using NexaMart.Application.DTOs.Common;
using NexaMart.Application.Interfaces.Services;
using NexaMart.Domain.Entities;
using NexaMart.Domain.Enums;

namespace NexaMart.Application.Services;

/// <summary>
/// Dedicated Store Administration Application Service.
/// Encapsulates all dashboard aggregation, search routing, and catalog orchestration.
/// </summary>
public class AdminService : IAdminService
{
    private readonly IProductService _productService;
    private readonly ICategoryService _categoryService;
    private readonly IOrderService _orderService;

    public AdminService(
        IProductService productService,
        ICategoryService categoryService,
        IOrderService orderService)
    {
        _productService = productService;
        _categoryService = categoryService;
        _orderService = orderService;
    }

    public async Task<AdminDashboardDto> GetDashboardAsync(CancellationToken cancellationToken = default)
    {
        var (totalProducts, activeProducts, lowStock, avgRating, totalReviews) = 
            await _productService.GetProductCatalogStatsAsync(cancellationToken);

        var categoriesWithStats = await _categoryService.GetAllCategoriesWithStatsAsync(cancellationToken);
        var recentProducts = await _productService.GetRecentProductsAsync(8, cancellationToken);
        var recentOrdersPaged = await _orderService.GetOrdersPagedAsync(pageIndex: 1, pageSize: 6, cancellationToken: cancellationToken);
        var (totalOrders, totalRevenue) = await _orderService.GetOrderStatsAsync(cancellationToken);

        return new AdminDashboardDto
        {
            TotalProducts = totalProducts,
            ActiveProductsCount = activeProducts,
            LowStockProductsCount = lowStock,
            TotalCategories = categoriesWithStats.Count,
            OverallAverageRating = avgRating,
            TotalReviewsCount = totalReviews,
            TotalOrders = totalOrders,
            TotalRevenue = totalRevenue,
            RecentProducts = recentProducts,
            CategoriesWithStats = categoriesWithStats,
            RecentOrders = recentOrdersPaged.Items
        };
    }

    public async Task<(string RouteAction, string RouteController, object? RouteValues, string? ErrorMessage)> ResolveSearchByIdAsync(
        int id,
        string? type = null,
        CancellationToken cancellationToken = default)
    {
        if (string.Equals(type, "Product", StringComparison.OrdinalIgnoreCase))
        {
            if (await _productService.ExistsAsync(id, cancellationToken))
            {
                return ("EditProduct", "Admin", new { id }, null);
            }
            return ("Products", "Admin", new { SearchId = id }, $"Product with ID #{id} was not found.");
        }

        if (string.Equals(type, "Category", StringComparison.OrdinalIgnoreCase))
        {
            if (await _categoryService.ExistsAsync(id, cancellationToken))
            {
                return ("EditCategory", "Admin", new { id }, null);
            }
            return ("Categories", "Admin", new { SearchId = id }, $"Category with ID #{id} was not found.");
        }

        if (string.Equals(type, "Order", StringComparison.OrdinalIgnoreCase))
        {
            var order = await _orderService.GetOrderByIdAsync(id, cancellationToken);
            if (order != null)
            {
                return ("Invoice", "Orders", new { id }, null);
            }
            return ("Orders", "Admin", new { searchId = id }, $"Order with ID #{id} was not found.");
        }

        // Auto-detect priority: Product -> Category -> Order
        if (await _productService.ExistsAsync(id, cancellationToken))
        {
            return ("EditProduct", "Admin", new { id }, null);
        }

        if (await _categoryService.ExistsAsync(id, cancellationToken))
        {
            return ("EditCategory", "Admin", new { id }, null);
        }

        var matchedOrder = await _orderService.GetOrderByIdAsync(id, cancellationToken);
        if (matchedOrder != null)
        {
            return ("Invoice", "Orders", new { id }, null);
        }

        return ("Dashboard", "Admin", null, $"No entity with ID #{id} found across Products, Categories, or Orders.");
    }

    public async Task<PagedResult<Product>> GetProductsPagedAsync(ProductFilterDto filter, CancellationToken cancellationToken = default)
    {
        return await _productService.GetProductsPagedAsync(filter, isStaff: true, cancellationToken);
    }

    public async Task<IReadOnlyList<Category>> GetActiveCategoriesAsync(CancellationToken cancellationToken = default)
    {
        return await _categoryService.GetAllActiveCategoriesAsync(cancellationToken);
    }

    public async Task<Product?> GetProductByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _productService.GetProductByIdAsync(id, isStaff: true, cancellationToken);
    }

    public async Task<Product> CreateProductAsync(Product product, CancellationToken cancellationToken = default)
    {
        product.AverageRating = 0.0m;
        product.ReviewCount = 0;
        return await _productService.CreateProductAsync(product, cancellationToken);
    }

    public async Task<Product> UpdateProductAsync(int id, Product product, CancellationToken cancellationToken = default)
    {
        return await _productService.UpdateProductAsync(id, product, cancellationToken);
    }

    public async Task<bool> ToggleProductStatusAsync(int id, CancellationToken cancellationToken = default)
    {
        var product = await _productService.GetProductByIdAsync(id, isStaff: true, cancellationToken);
        if (product == null) return false;

        product.IsActive = !product.IsActive;
        await _productService.UpdateProductAsync(id, product, cancellationToken);
        return product.IsActive;
    }

    public async Task<bool> DeleteProductAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _productService.DeleteProductAsync(id, cancellationToken);
    }

    public async Task<PagedResult<Category>> GetCategoriesPagedAsync(CategoryFilterDto filter, CancellationToken cancellationToken = default)
    {
        return await _categoryService.GetCategoriesPagedAsync(filter, isStaff: true, cancellationToken);
    }

    public async Task<Category?> GetCategoryByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _categoryService.GetCategoryByIdAsync(id, isStaff: true, cancellationToken);
    }

    public async Task<Category> CreateCategoryAsync(Category category, CancellationToken cancellationToken = default)
    {
        return await _categoryService.CreateCategoryAsync(category, cancellationToken);
    }

    public async Task<Category> UpdateCategoryAsync(int id, Category category, CancellationToken cancellationToken = default)
    {
        return await _categoryService.UpdateCategoryAsync(id, category, cancellationToken);
    }

    public async Task<bool> ToggleCategoryStatusAsync(int id, CancellationToken cancellationToken = default)
    {
        var category = await _categoryService.GetCategoryByIdAsync(id, isStaff: true, cancellationToken);
        if (category == null) return false;

        category.IsActive = !category.IsActive;
        await _categoryService.UpdateCategoryAsync(id, category, cancellationToken);
        return category.IsActive;
    }

    public async Task<bool> DeleteCategoryAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _categoryService.DeleteCategoryAsync(id, cancellationToken);
    }

    public async Task<PagedResult<Order>> GetOrdersPagedAsync(
        OrderStatus? status,
        int? searchId,
        string? searchTerm,
        int pageIndex = 1,
        int pageSize = 15,
        CancellationToken cancellationToken = default)
    {
        return await _orderService.GetOrdersPagedAsync(
            status: status,
            searchTerm: searchTerm,
            searchId: searchId,
            pageIndex: pageIndex,
            pageSize: pageSize,
            cancellationToken: cancellationToken);
    }

    public async Task<bool> UpdateOrderStatusAsync(int orderId, OrderStatus status, CancellationToken cancellationToken = default)
    {
        return await _orderService.UpdateOrderStatusAsync(orderId, status, cancellationToken);
    }
}
