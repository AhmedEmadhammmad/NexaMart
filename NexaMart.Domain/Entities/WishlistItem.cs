namespace NexaMart.Domain.Entities;

/// <summary>
/// Represents a product saved by a customer to their wishlist.
/// </summary>
public class WishlistItem
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public int UserId { get; set; }
    public ApplicationUser User { get; set; } = null!;

    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;
}
