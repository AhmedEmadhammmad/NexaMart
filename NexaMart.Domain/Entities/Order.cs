using NexaMart.Domain.Enums;

namespace NexaMart.Domain.Entities;

/// <summary>
/// Customer purchase order capturing financial totals and cancellation audit.
/// </summary>
public class Order
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public string OrderNumber { get; set; } = string.Empty; // e.g. NXM-20260001
    public int UserId { get; set; }
    public ApplicationUser User { get; set; } = null!;

    // Order Pricing Totals
    public decimal SubTotal { get; set; } // Sum of product line totals
    public decimal TotalAmount { get; set; } // Total amount payable

    // Order Lifecycle and Cancellation Tracking
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public string? CancellationReason { get; set; } // Detailed explanation for cancellation
    public DateTime? CancelledAt { get; set; } // Timestamp when cancelled
    public int? CancelledByUserId { get; set; } // User identifier who triggered cancellation

    // Navigation properties
    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}
