namespace NexaMart.Domain.Entities;

/// <summary>
/// Verified customer product review with 1 to 5 star rating and optional feedback.
/// </summary>
public class Review
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;

    public int UserId { get; set; }
    public ApplicationUser User { get; set; } = null!;

    public int Rating { get; set; } = 5;
    public string? Comment { get; set; }
}
