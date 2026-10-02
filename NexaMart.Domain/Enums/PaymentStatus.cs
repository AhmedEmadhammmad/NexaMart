namespace NexaMart.Domain.Enums;

/// <summary>
/// Lifecycle states of an order's financial transaction.
/// </summary>
public enum PaymentStatus
{
    Pending = 0,
    Paid = 1,
    Failed = 2,
    Refunded = 3
}
