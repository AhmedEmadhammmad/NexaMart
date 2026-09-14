namespace NexaMart.Application.DTOs.Common;

/// <summary>
/// Common pagination and search filter model.
/// </summary>
public class PaginationFilter
{
    public string? SearchTerm { get; set; }
    public int? SearchId { get; set; }
    public bool? IsActiveOnly { get; set; } = true;
    public int PageIndex { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
