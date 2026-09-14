namespace NexaMart.Web.Models;

public class WishlistItemViewModel
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? ProductImageUrl { get; set; }
    public decimal Price { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public bool InStock { get; set; }
    public int AvailableStock { get; set; }
}

public class WishlistViewModel
{
    public IReadOnlyList<WishlistItemViewModel> Items { get; set; } = new List<WishlistItemViewModel>();
    public int TotalCount => Items.Count;
    public bool IsEmpty => Items.Count == 0;
}
