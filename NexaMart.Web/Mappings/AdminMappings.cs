using Microsoft.AspNetCore.Mvc.Rendering;
using NexaMart.Application.Common.Models;
using NexaMart.Application.DTOs.Admin;
using NexaMart.Application.DTOs.Common;
using NexaMart.Domain.Entities;
using NexaMart.Domain.Enums;
using NexaMart.Web.Models;

namespace NexaMart.Web.Mappings;

/// <summary>
/// Admin mapping extensions for Store Management (Products, Categories, Orders, Dashboard).
/// </summary>
public static partial class MappingExtensions
{
    public static AdminProductListItemViewModel ToAdminProductListItemViewModel(this Product product)
    {
        return new AdminProductListItemViewModel
        {
            Id = product.Id,
            Name = product.Name,
            ImageUrl = product.ImageUrl,
            CategoryId = product.CategoryId,
            CategoryName = product.Category?.Name ?? string.Empty,
            Price = product.Price,
            StockQuantity = product.StockQuantity,
            IsActive = product.IsActive,
            AverageRating = product.AverageRating,
            ReviewCount = product.ReviewCount,
            CreatedAt = product.CreatedAt
        };
    }

    public static AdminCategoryListItemViewModel ToAdminCategoryListItemViewModel(this Category category)
    {
        return new AdminCategoryListItemViewModel
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description,
            ImageUrl = category.ImageUrl,
            DisplayOrder = category.DisplayOrder,
            IsActive = category.IsActive,
            ProductCount = category.Products?.Count ?? 0,
            AverageRating = category.AverageRating
        };
    }

    public static AdminProductListViewModel ToAdminProductListViewModel(
        this PagedResult<Product> pagedProducts,
        ProductFilterDto filter,
        IReadOnlyList<Category> categories)
    {
        return new AdminProductListViewModel
        {
            Products = new PagedResult<AdminProductListItemViewModel>(
                pagedProducts.Items.Select(p => p.ToAdminProductListItemViewModel()).ToList(),
                pagedProducts.TotalCount,
                pagedProducts.PageIndex,
                pagedProducts.PageSize),
            Filter = filter,
            Categories = categories.Select(c => c.ToCardViewModel()).ToList()
        };
    }

    public static AdminCategoryListViewModel ToAdminCategoryListViewModel(
        this PagedResult<Category> pagedCategories,
        CategoryFilterDto filter)
    {
        return new AdminCategoryListViewModel
        {
            Categories = new PagedResult<AdminCategoryListItemViewModel>(
                pagedCategories.Items.Select(c => c.ToAdminCategoryListItemViewModel()).ToList(),
                pagedCategories.TotalCount,
                pagedCategories.PageIndex,
                pagedCategories.PageSize),
            Filter = filter
        };
    }

    public static AdminOrderListItemViewModel ToAdminOrderListItemViewModel(this Order order)
    {
        return new AdminOrderListItemViewModel
        {
            Id = order.Id,
            OrderNumber = order.OrderNumber,
            CustomerName = order.User?.FullName ?? "Unknown",
            CustomerEmail = order.User?.Email ?? string.Empty,
            OrderDate = order.CreatedAt,
            Status = order.Status,
            TotalAmount = order.TotalAmount,
            ItemsCount = order.OrderItems?.Count ?? 0
        };
    }

    public static AdminOrderListViewModel ToAdminOrderListViewModel(
        this PagedResult<Order> pagedOrders,
        OrderStatus? statusFilter = null,
        int? searchId = null,
        string? searchTerm = null)
    {
        return new AdminOrderListViewModel
        {
            Orders = new PagedResult<AdminOrderListItemViewModel>(
                pagedOrders.Items.Select(o => o.ToAdminOrderListItemViewModel()).ToList(),
                pagedOrders.TotalCount,
                pagedOrders.PageIndex,
                pagedOrders.PageSize),
            StatusFilter = statusFilter,
            SearchId = searchId,
            SearchTerm = searchTerm
        };
    }

    public static ProductFormViewModel ToFormViewModel(this Product product, IEnumerable<SelectListItem>? categoriesList = null)
    {
        return new ProductFormViewModel
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            ImageUrl = product.ImageUrl,
            Price = product.Price,
            StockQuantity = product.StockQuantity,
            CategoryId = product.CategoryId,
            IsActive = product.IsActive,
            CategoriesList = categoriesList
        };
    }

    public static Product ToEntity(this ProductFormViewModel model)
    {
        return new Product
        {
            Id = model.Id ?? 0,
            Name = model.Name.Trim(),
            Description = model.Description?.Trim(),
            ImageUrl = model.ImageUrl?.Trim(),
            Price = model.Price,
            StockQuantity = model.StockQuantity,
            CategoryId = model.CategoryId,
            IsActive = model.IsActive
        };
    }

    public static CategoryFormViewModel ToFormViewModel(this Category category)
    {
        return new CategoryFormViewModel
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description,
            ImageUrl = category.ImageUrl,
            DisplayOrder = category.DisplayOrder,
            IsActive = category.IsActive
        };
    }

    public static Category ToEntity(this CategoryFormViewModel model)
    {
        return new Category
        {
            Id = model.Id ?? 0,
            Name = model.Name.Trim(),
            Description = model.Description?.Trim(),
            ImageUrl = model.ImageUrl?.Trim(),
            DisplayOrder = model.DisplayOrder,
            IsActive = model.IsActive
        };
    }

    public static AdminDashboardViewModel ToViewModel(this AdminDashboardDto dto)
    {
        return new AdminDashboardViewModel
        {
            TotalProducts = dto.TotalProducts,
            ActiveProductsCount = dto.ActiveProductsCount,
            LowStockProductsCount = dto.LowStockProductsCount,
            TotalCategories = dto.TotalCategories,
            OverallAverageRating = dto.OverallAverageRating,
            TotalReviewsCount = dto.TotalReviewsCount,
            TotalOrders = dto.TotalOrders,
            TotalRevenue = dto.TotalRevenue,
            RecentProducts = dto.RecentProducts.Select(p => p.ToAdminProductListItemViewModel()).ToList(),
            CategoryStats = dto.CategoriesWithStats.Select(c => c.ToAdminCategoryListItemViewModel()).ToList(),
            RecentOrders = dto.RecentOrders.Select(o => o.ToAdminOrderListItemViewModel()).ToList()
        };
    }
}
