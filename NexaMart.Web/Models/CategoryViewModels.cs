using System.ComponentModel.DataAnnotations;
using NexaMart.Application.Common.Models;
using NexaMart.Application.DTOs.Common;

namespace NexaMart.Web.Models;

public class CategoryCardViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }
    public int ProductCount { get; set; }
}

public class CategoryListViewModel
{
    public PagedResult<CategoryCardViewModel> Categories { get; set; } = new(new List<CategoryCardViewModel>(), 0, 1, 10);
    public CategoryFilterDto Filter { get; set; } = new();
}

public class CategoryFormViewModel
{
    public int? Id { get; set; }

    [Required(ErrorMessage = "Category name is required.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Category name must be between 2 and 100 characters.")]
    [Display(Name = "Category Name")]
    public string Name { get; set; } = string.Empty;

    [Display(Name = "Description")]
    public string? Description { get; set; }

    [Url(ErrorMessage = "Please enter a valid image URL.")]
    [Display(Name = "Image URL")]
    public string? ImageUrl { get; set; }

    [Display(Name = "Display Order")]
    [Range(0, 1000, ErrorMessage = "Display order must be between 0 and 1000.")]
    public int DisplayOrder { get; set; } = 0;

    [Display(Name = "Active")]
    public bool IsActive { get; set; } = true;
}
