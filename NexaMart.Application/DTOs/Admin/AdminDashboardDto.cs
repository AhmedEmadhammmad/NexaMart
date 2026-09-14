using NexaMart.Domain.Entities;

namespace NexaMart.Application.DTOs.Admin;

/// <summary>
/// Aggregated catalog and store metrics DTO for the Store Admin Dashboard.
/// </summary>
public class AdminDashboardDto
{
    public int TotalProducts { get; set; }
    public int ActiveProductsCount { get; set; }
    public int LowStockProductsCount { get; set; }
    public int TotalCategories { get; set; }
    public decimal OverallAverageRating { get; set; }
    public int TotalReviewsCount { get; set; }
    public int TotalOrders { get; set; }
    public decimal TotalRevenue { get; set; }

    public IReadOnlyList<Product> RecentProducts { get; set; } = new List<Product>();
    public IReadOnlyList<Category> CategoriesWithStats { get; set; } = new List<Category>();
    public IReadOnlyList<Order> RecentOrders { get; set; } = new List<Order>();
}
