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

    // Financial Breakdown (Ready for Paymob & Accounting)
    public decimal SubTotal { get; set; } // Sum of product line totals
    public decimal ShippingCost { get; set; } = 0.0m; // Delivery charges
    public decimal TaxAmount { get; set; } = 0.0m; // Applicable tax / VAT
    public decimal DiscountAmount { get; set; } = 0.0m; // Discounts or coupons applied
    public decimal TotalAmount { get; set; } // Final payable amount: SubTotal + ShippingCost + TaxAmount - DiscountAmount
    public string Currency { get; set; } = "EGP"; // Default currency (Egyptian Pound for Paymob)

    // Payment Processing (Paymob Integration)
    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.CashOnDelivery;
    public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;
    public string? TransactionId { get; set; } // Paymob Transaction ID / Order Reference
    public DateTime? PaidAt { get; set; } // Timestamp when transaction was captured

    // Shipping & Billing Snapshot (Paymob Billing Data & Delivery Fulfillment)
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public string ShippingAddress { get; set; } = string.Empty;
    public string City { get; set; } = "Cairo";
    public string State { get; set; } = "Cairo";
    public string? PostalCode { get; set; }
    public string? OrderNotes { get; set; }

    // Order Lifecycle and Cancellation Tracking
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public string? CancellationReason { get; set; } // Detailed explanation for cancellation
    public DateTime? CancelledAt { get; set; } // Timestamp when cancelled
    public int? CancelledByUserId { get; set; } // User identifier who triggered cancellation

    // Navigation properties
    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}
