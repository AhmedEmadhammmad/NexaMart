using NexaMart.Application.Common.Models;
using NexaMart.Domain.Entities;
using NexaMart.Domain.Enums;

namespace NexaMart.Application.DTOs.SuperAdmin;

/// <summary>
/// Aggregated user list with search parameters and tab counts for SuperAdmin governance.
/// </summary>
public class SuperAdminUserListDto
{
    public PagedResult<ApplicationUser> Users { get; set; } = new(new List<ApplicationUser>(), 0, 1, 15);
    public string? SearchTerm { get; set; }
    public UserRoleType? RoleFilter { get; set; }
    public bool? IsActiveFilter { get; set; }

    public int AllUsersCount { get; set; }
    public int CustomersCount { get; set; }
    public int AdminsCount { get; set; }
    public int BlockedCount { get; set; }
}
