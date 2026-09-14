namespace NexaMart.Domain.Entities;

/// <summary>
/// Line item within an order preserving an immutable financial snapshot of product, price, and cost.
/// </summary>
public class OrderItem
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public int OrderId { get; set; }
    public Order Order { get; set; } = null!;

    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;

    // Historical Product Snapshot
    public string ProductName { get; set; } = string.Empty;
    public string? ProductImageUrl { get; set; }

    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public decimal TotalPrice { get; set; }

    public bool IsCancelled { get; set; } = false;
}
