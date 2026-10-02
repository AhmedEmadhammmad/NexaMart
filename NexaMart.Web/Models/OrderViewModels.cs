using NexaMart.Application.Common.Models;
using NexaMart.Domain.Enums;

namespace NexaMart.Web.Models;

public class OrderItemViewModel
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? ProductImageUrl { get; set; }
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public decimal TotalPrice { get; set; }
    public bool IsCancelled { get; set; }
}

public class OrderDetailsViewModel
{
    public int Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;
    public string? UserPhone { get; set; }

    // Shipping & Billing Details
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public string ShippingAddress { get; set; } = string.Empty;
    public string City { get; set; } = "Cairo";
    public string State { get; set; } = "Cairo";
    public string? PostalCode { get; set; }
    public string? OrderNotes { get; set; }

    // Financial Breakdown & Currency
    public decimal SubTotal { get; set; }
    public decimal ShippingCost { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public string Currency { get; set; } = "EGP";

    // Payment Processing (Paymob Readiness)
    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.CashOnDelivery;
    public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;
    public string? TransactionId { get; set; }
    public DateTime? PaidAt { get; set; }

    public DateTime OrderDate { get; set; }
    public OrderStatus Status { get; set; }
    public string? CancellationReason { get; set; }
    public DateTime? CancelledAt { get; set; }
    public IReadOnlyList<OrderItemViewModel> OrderItems { get; set; } = new List<OrderItemViewModel>();
    public int TotalItemsCount => OrderItems.Sum(i => i.Quantity);
}

public class OrderListViewModel
{
    public PagedResult<OrderDetailsViewModel> Orders { get; set; } = new(new List<OrderDetailsViewModel>(), 0, 1, 10);
    public OrderStatus? StatusFilter { get; set; }
}

public class CheckoutViewModel
{
    [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Full Name is required.")]
    [System.ComponentModel.DataAnnotations.StringLength(100, MinimumLength = 3)]
    public string CustomerName { get; set; } = string.Empty;

    [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Email address is required.")]
    [System.ComponentModel.DataAnnotations.EmailAddress]
    public string CustomerEmail { get; set; } = string.Empty;

    [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Phone number is required.")]
    [System.ComponentModel.DataAnnotations.RegularExpression(@"^\+?[0-9\s\-()]{7,20}$", ErrorMessage = "Please enter a valid phone number.")]
    public string CustomerPhone { get; set; } = string.Empty;

    [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Shipping address is required.")]
    [System.ComponentModel.DataAnnotations.StringLength(300)]
    public string ShippingAddress { get; set; } = string.Empty;

    [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "City is required.")]
    [System.ComponentModel.DataAnnotations.StringLength(100)]
    public string City { get; set; } = "Cairo";

    [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "State / Governorate is required.")]
    [System.ComponentModel.DataAnnotations.StringLength(100)]
    public string State { get; set; } = "Cairo";

    public string? PostalCode { get; set; }
    public string? OrderNotes { get; set; }

    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.CashOnDelivery;

    // Financial calculations
    public decimal SubTotal { get; set; }
    public decimal ShippingCost { get; set; } = 0.0m;
    public decimal TaxAmount { get; set; } = 0.0m;
    public decimal DiscountAmount { get; set; } = 0.0m;
    public decimal TotalAmount => Math.Max(0.0m, SubTotal + ShippingCost + TaxAmount - DiscountAmount);
    public string Currency { get; set; } = "EGP";

    public IReadOnlyList<CartItemViewModel> CartItems { get; set; } = new List<CartItemViewModel>();
}
