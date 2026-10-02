using NexaMart.Domain.Enums;

namespace NexaMart.Application.Common.Models;

/// <summary>
/// Encapsulates customer checkout payload with shipping and payment details ready for Paymob integration.
/// </summary>
public class CreateOrderRequest
{
    public int UserId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public string ShippingAddress { get; set; } = string.Empty;
    public string City { get; set; } = "Cairo";
    public string State { get; set; } = "Cairo";
    public string? PostalCode { get; set; }
    public string? OrderNotes { get; set; }
    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.CashOnDelivery;
    public decimal ShippingCost { get; set; } = 0.0m;
    public decimal TaxAmount { get; set; } = 0.0m;
    public decimal DiscountAmount { get; set; } = 0.0m;
}
