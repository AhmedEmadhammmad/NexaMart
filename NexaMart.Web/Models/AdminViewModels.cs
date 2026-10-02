using NexaMart.Application.Common.Models;
using NexaMart.Application.DTOs.Common;
using NexaMart.Domain.Enums;

namespace NexaMart.Web.Models;

/// <summary>
/// ViewModels exclusively for Store Admin (Catalog, Products, Categories, Orders management).
/// All views are strictly tabular with direct ID search capabilities and zero-out rating integration.
/// </summary>
public class AdminDashboardViewModel
{
    // High-Level KPIs & Metrics
    public int TotalProducts { get; set; }
    public int ActiveProductsCount { get; set; }
    public int LowStockProductsCount { get; set; }
    public int TotalCategories { get; set; }
    public decimal OverallAverageRating { get; set; }
    public int TotalReviewsCount { get; set; }
    public int TotalOrders { get; set; }
    public decimal TotalRevenue { get; set; }

    // Quick Search by ID
    public int? SearchId { get; set; }
    public string? SearchType { get; set; } = "All";

    // Tabular Data Sets (STRICTLY tables, NOT card view)
    public IReadOnlyList<AdminProductListItemViewModel> RecentProducts { get; set; } = new List<AdminProductListItemViewModel>();
    public IReadOnlyList<AdminCategoryListItemViewModel> CategoryStats { get; set; } = new List<AdminCategoryListItemViewModel>();
    public IReadOnlyList<AdminOrderListItemViewModel> RecentOrders { get; set; } = new List<AdminOrderListItemViewModel>();
}

public class AdminProductListItemViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public bool IsActive { get; set; }
    public decimal AverageRating { get; set; }
    public int ReviewCount { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class AdminProductListViewModel
{
    public PagedResult<AdminProductListItemViewModel> Products { get; set; } = new(new List<AdminProductListItemViewModel>(), 0, 1, 15);
    public ProductFilterDto Filter { get; set; } = new();
    public IReadOnlyList<CategoryCardViewModel> Categories { get; set; } = new List<CategoryCardViewModel>();
}

public class AdminCategoryListItemViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }
    public int ProductCount { get; set; }
    public decimal AverageRating { get; set; }
}

public class AdminCategoryListViewModel
{
    public PagedResult<AdminCategoryListItemViewModel> Categories { get; set; } = new(new List<AdminCategoryListItemViewModel>(), 0, 1, 15);
    public CategoryFilterDto Filter { get; set; } = new();
}

public class AdminOrderListItemViewModel
{
    public int Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    public OrderStatus Status { get; set; }
    public decimal SubTotal { get; set; }
    public decimal ShippingCost { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public string Currency { get; set; } = "EGP";
    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.CashOnDelivery;
    public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;
    public string? TransactionId { get; set; }
    public int ItemsCount { get; set; }
}

public class AdminOrderListViewModel
{
    public PagedResult<AdminOrderListItemViewModel> Orders { get; set; } = new(new List<AdminOrderListItemViewModel>(), 0, 1, 15);
    public OrderStatus? StatusFilter { get; set; }
    public int? SearchId { get; set; }
    public string? SearchTerm { get; set; }
}

