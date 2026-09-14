using NexaMart.Domain.Entities;

namespace NexaMart.Application.DTOs.SuperAdmin;

/// <summary>
/// Aggregated platform authority metrics DTO for the Executive SuperAdmin Dashboard.
/// </summary>
public class SuperAdminDashboardDto
{
    public int TotalUsers { get; set; }
    public int TotalCustomers { get; set; }
    public int TotalAdmins { get; set; }
    public int TotalSuperAdmins { get; set; }
    public int BlockedUsersCount { get; set; }

    public int TotalProducts { get; set; }
    public int TotalCategories { get; set; }
    public int TotalOrders { get; set; }
    public decimal TotalRevenue { get; set; }

    public IReadOnlyList<ApplicationUser> RecentRegisteredUsers { get; set; } = new List<ApplicationUser>();
    public IReadOnlyList<Order> RecentOrders { get; set; } = new List<Order>();
}
