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
    public string ShippingAddress { get; set; } = "123 Business Way, Cairo, Egypt";
    public string PaymentMethod { get; set; } = "Cash on Delivery / Electronic";
    public DateTime OrderDate { get; set; }
    public OrderStatus Status { get; set; }
    public decimal SubTotal { get; set; }
    public decimal ShippingFee => SubTotal > 100 || OrderItems.Count == 0 ? 0.00m : 15.00m;
    public decimal TotalAmount { get; set; }
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
