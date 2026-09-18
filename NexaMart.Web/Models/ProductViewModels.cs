using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using NexaMart.Application.Common.Models;
using NexaMart.Application.DTOs.Common;

namespace NexaMart.Web.Models;

public class ProductCardViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string? ImageUrl { get; set; }
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public decimal AverageRating { get; set; }
    public int ReviewCount { get; set; }
    public int StockQuantity { get; set; }
    public bool IsActive { get; set; }
    public bool InStock => StockQuantity > 0;
}

public class ProductDetailsViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public string? ImageUrl { get; set; }
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public decimal AverageRating { get; set; }
    public int ReviewCount { get; set; }
    public int StockQuantity { get; set; }
    public bool IsActive { get; set; }
    public bool InStock => StockQuantity > 0;

    public bool IsInWishlist { get; set; }
    public int CurrentCartQuantity { get; set; }

    public IReadOnlyList<ReviewItemViewModel> Reviews { get; set; } = new List<ReviewItemViewModel>();
    public IReadOnlyList<ProductCardViewModel> RelatedProducts { get; set; } = new List<ProductCardViewModel>();
    public AddReviewViewModel NewReview { get; set; } = new();
}

public class ProductListViewModel
{
    public PagedResult<ProductCardViewModel> Products { get; set; } = new(new List<ProductCardViewModel>(), 0, 1, 12);
    public ProductFilterDto Filter { get; set; } = new();
    public IReadOnlyList<CategoryCardViewModel> Categories { get; set; } = new List<CategoryCardViewModel>();
}

public class ProductFormViewModel
{
    public int? Id { get; set; }

    [Required(ErrorMessage = "Product name is required.")]
    [StringLength(200, MinimumLength = 2, ErrorMessage = "Product name must be between 2 and 200 characters.")]
    [RegularExpression(@"^[^<>\/\\\{\}\[\]]*$", ErrorMessage = "Product name cannot contain script tags or special code characters.")]
    [Display(Name = "Product Name")]
    public string Name { get; set; } = string.Empty;

    [MaxLength(2000, ErrorMessage = "Description cannot exceed 2000 characters.")]
    [RegularExpression(@"^[^<>{}]*$", ErrorMessage = "Description cannot contain script tags or braces.")]
    [Display(Name = "Description")]
    public string? Description { get; set; }

    /// <summary>
    /// File uploaded via Drag & Drop or file chooser.
    /// </summary>
    [Display(Name = "Product Image")]
    public IFormFile? ImageFile { get; set; }

    /// <summary>
    /// Preserved relative image URL when editing or after upload.
    /// </summary>
    public string? ImageUrl { get; set; }

    [Required(ErrorMessage = "Price is required.")]
    [Range(0.01, 1000000.00, ErrorMessage = "Price must be between $0.01 and $1,000,000.00.")]
    [Display(Name = "Price ($)")]
    public decimal Price { get; set; }

    [Required(ErrorMessage = "Stock quantity is required.")]
    [Range(0, 100000, ErrorMessage = "Stock quantity must be between 0 and 100,000 units.")]
    [Display(Name = "Stock Quantity")]
    public int StockQuantity { get; set; }

    [Required(ErrorMessage = "Category is required.")]
    [Range(1, int.MaxValue, ErrorMessage = "Please select a valid category.")]
    [Display(Name = "Category")]
    public int CategoryId { get; set; }

    [Display(Name = "Active")]
    public bool IsActive { get; set; } = true;

    public IEnumerable<SelectListItem>? CategoriesList { get; set; }
}
