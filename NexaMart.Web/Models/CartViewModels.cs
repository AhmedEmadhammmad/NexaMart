namespace NexaMart.Web.Models;

public class CartItemViewModel
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? ProductImageUrl { get; set; }
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public decimal TotalPrice => UnitPrice * Quantity;
    public int AvailableStock { get; set; }
}

public class CartViewModel
{
    public IReadOnlyList<CartItemViewModel> Items { get; set; } = new List<CartItemViewModel>();
    public decimal SubTotal => Items.Sum(i => i.TotalPrice);
    public decimal ShippingFee => SubTotal > 100 || Items.Count == 0 ? 0.00m : 15.00m;
    public decimal TotalAmount => SubTotal + ShippingFee;
    public int TotalCount => Items.Sum(i => i.Quantity);
    public bool IsEmpty => Items.Count == 0;
}
