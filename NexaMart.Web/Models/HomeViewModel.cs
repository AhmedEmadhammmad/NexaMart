namespace NexaMart.Web.Models;

public class HomeViewModel
{
    public IReadOnlyList<CategoryCardViewModel> FeaturedCategories { get; set; } = new List<CategoryCardViewModel>();
    public IReadOnlyList<ProductCardViewModel> TopSellingProducts { get; set; } = new List<ProductCardViewModel>();
    public IReadOnlyList<ProductCardViewModel> LatestProducts { get; set; } = new List<ProductCardViewModel>();
}
