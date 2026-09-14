using NexaMart.Domain.Enums;

namespace NexaMart.Domain.Entities;

/// <summary>
/// Custom application user entity with independent authentication and role-based permissions.
/// Supports roles: SuperAdmin, Admin, and Customer.
/// </summary>
public class ApplicationUser
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }

    public UserRoleType RoleType { get; set; } = UserRoleType.Customer;
    public bool IsActive { get; set; } = true;
    public DateTime? LastLoginAt { get; set; }

    // JWT Refresh Token fields
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiresAt { get; set; }

    // Navigation properties
    public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
    public ICollection<WishlistItem> WishlistItems { get; set; } = new List<WishlistItem>();
    public ICollection<Order> Orders { get; set; } = new List<Order>();
    public ICollection<Review> Reviews { get; set; } = new List<Review>();
}
