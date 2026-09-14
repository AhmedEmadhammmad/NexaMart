namespace NexaMart.Domain.Entities;

/// <summary>
/// Category entity for organizing catalog products and performance metrics.
/// </summary>
public class Category
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsActive { get; set; } = true;
    public int DisplayOrder { get; set; } = 0;

    // Quality ratings
    public decimal AverageRating { get; set; } = 0.0m;

    // Associated Products
    public ICollection<Product> Products { get; set; } = new List<Product>();
}
