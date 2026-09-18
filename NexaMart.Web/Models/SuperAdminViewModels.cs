using System.ComponentModel.DataAnnotations;
using NexaMart.Application.Common.Models;
using NexaMart.Domain.Enums;

namespace NexaMart.Web.Models;

/// <summary>
/// ViewModels exclusively for SuperAdmin (Full site control, User Management, Role Promotion, Global Analytics).
/// </summary>
public class SuperAdminDashboardViewModel
{
    // User Metrics (SuperAdmin Exclusive)
    public int TotalUsers { get; set; }
    public int TotalCustomers { get; set; }
    public int TotalAdmins { get; set; }
    public int TotalSuperAdmins { get; set; }
    public int BlockedUsersCount { get; set; }

    // Whole-Site Global Overview (SuperAdmin sees everything)
    public int TotalProducts { get; set; }
    public int TotalCategories { get; set; }
    public int TotalOrders { get; set; }
    public decimal TotalRevenue { get; set; }

    // Recent Activity
    public IReadOnlyList<SuperAdminUserListItemViewModel> RecentRegisteredUsers { get; set; } = new List<SuperAdminUserListItemViewModel>();
    public IReadOnlyList<OrderDetailsViewModel> RecentOrders { get; set; } = new List<OrderDetailsViewModel>();
}

public class SuperAdminUserListItemViewModel
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public UserRoleType RoleType { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public int OrdersCount { get; set; }
    public decimal TotalSpent { get; set; }
}

public class SuperAdminUserListViewModel
{
    public PagedResult<SuperAdminUserListItemViewModel> Users { get; set; } = new(new List<SuperAdminUserListItemViewModel>(), 0, 1, 15);
    public string? SearchTerm { get; set; }
    public UserRoleType? RoleFilter { get; set; }
    public bool? IsActiveFilter { get; set; }

    // Counter badges for tabs
    public int AllUsersCount { get; set; }
    public int CustomersCount { get; set; }
    public int AdminsCount { get; set; }
    public int BlockedCount { get; set; }
}

public class SuperAdminUserDetailsViewModel
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public UserRoleType RoleType { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public IReadOnlyList<OrderDetailsViewModel> Orders { get; set; } = new List<OrderDetailsViewModel>();
    public decimal TotalSpent => Orders.Sum(o => o.TotalAmount);
}

public class PromoteUserRoleViewModel
{
    [Required]
    public int UserId { get; set; }

    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public UserRoleType CurrentRole { get; set; }

    [Required(ErrorMessage = "Please select a new role.")]
    [Display(Name = "New Role")]
    public UserRoleType NewRole { get; set; }
}

public class CreateAdminAccountViewModel
{
    [Required(ErrorMessage = "Full Name is required.")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "Full Name must be between 3 and 100 characters.")]
    [RegularExpression(@"^[a-zA-Z\u0600-\u06FF\s.'-]+$", ErrorMessage = "Full Name can only contain letters, spaces, hyphens, and apostrophes.")]
    [Display(Name = "Full Name")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required.")]
    [MaxLength(150, ErrorMessage = "Email cannot exceed 150 characters.")]
    [RegularExpression(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", ErrorMessage = "Please enter a valid email address with a valid domain (e.g. user@domain.com).")]
    [Display(Name = "Email Address")]
    public string Email { get; set; } = string.Empty;

    [RegularExpression(@"^\+?[0-9\s\-()]{7,20}$", ErrorMessage = "Please enter a valid phone number (7 to 20 digits).")]
    [Display(Name = "Phone Number")]
    public string? PhoneNumber { get; set; }

    [Required(ErrorMessage = "Role is required.")]
    [Display(Name = "Role")]
    public UserRoleType RoleType { get; set; } = UserRoleType.Admin;

    [Required(ErrorMessage = "Password is required.")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be at least 8 characters long.")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&#._-])[A-Za-z\d@$!%*?&#._-]{8,}$", 
        ErrorMessage = "Password must contain at least 1 uppercase letter, 1 lowercase letter, 1 number, and 1 special character (@$!%*?&#._-).")]
    [DataType(DataType.Password)]
    [Display(Name = "Password")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Confirm Password is required.")]
    [DataType(DataType.Password)]
    [Compare(nameof(Password), ErrorMessage = "Passwords do not match.")]
    [Display(Name = "Confirm Password")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
