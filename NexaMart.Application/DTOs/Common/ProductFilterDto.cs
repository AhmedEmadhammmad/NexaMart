namespace NexaMart.Application.DTOs.Common;

/// <summary>
/// Product pagination, search, category, price, and sorting filter parameters.
/// </summary>
public class ProductFilterDto : PaginationFilter
{
    public int? CategoryId { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public string? SortBy { get; set; }
}
