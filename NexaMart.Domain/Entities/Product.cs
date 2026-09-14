namespace NexaMart.Domain.Entities;

/// <summary>
/// Product catalog item with direct image, pricing, cost, stock, and sales performance analytics.
/// </summary>
public class Product
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public int CategoryId { get; set; }
    public Category Category { get; set; } = null!;

    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }

    // Pricing and Inventory
    public decimal Price { get; set; }
    public int StockQuantity { get; set; } = 0;

    // Customer Reviews
    public decimal AverageRating { get; set; } = 0.0m;
    public int ReviewCount { get; set; } = 0;

    public bool IsActive { get; set; } = true;

    // Navigation properties
    public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
    public ICollection<WishlistItem> WishlistItems { get; set; } = new List<WishlistItem>();
    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    public ICollection<Review> Reviews { get; set; } = new List<Review>();
}
