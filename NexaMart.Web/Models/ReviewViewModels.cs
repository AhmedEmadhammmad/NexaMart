using System.ComponentModel.DataAnnotations;

namespace NexaMart.Web.Models;

public class ReviewItemViewModel
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Rating { get; set; }
    public string? Comment { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class AddReviewViewModel
{
    [Required]
    public int ProductId { get; set; }

    [Required(ErrorMessage = "Please select a star rating.")]
    [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5 stars.")]
    [Display(Name = "Rating")]
    public int Rating { get; set; } = 5;

    [StringLength(1000, ErrorMessage = "Comment cannot exceed 1000 characters.")]
    [RegularExpression(@"^[^<>{}]*$", ErrorMessage = "Comment cannot contain HTML or script tags.")]
    [Display(Name = "Your Review")]
    public string? Comment { get; set; }
}
