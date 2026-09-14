using NexaMart.Application.Common.Models;
using NexaMart.Application.DTOs.Common;
using NexaMart.Domain.Entities;

namespace NexaMart.Application.Interfaces.Services;

/// <summary>
/// Service contract for complete product catalog management and queries.
/// </summary>
public interface IProductService
{
    Task<PagedResult<Product>> GetProductsPagedAsync(ProductFilterDto filter, bool isStaff = false, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Product>> GetAllProductsAsync(bool activeOnly = false, CancellationToken cancellationToken = default);
    Task<Product?> GetProductByIdAsync(int id, bool isStaff = false, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Product>> GetRelatedProductsAsync(int productId, int categoryId, int count = 4, CancellationToken cancellationToken = default);
    Task<Product> CreateProductAsync(Product product, CancellationToken cancellationToken = default);
    Task<Product> UpdateProductAsync(int id, Product product, CancellationToken cancellationToken = default);
    Task<bool> DeleteProductAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Product>> GetTopSellingProductsAsync(int count = 10, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default);
    Task<(int TotalProducts, int ActiveProducts, int LowStock, decimal AvgRating, int TotalReviews)> GetProductCatalogStatsAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Product>> GetRecentProductsAsync(int count = 10, CancellationToken cancellationToken = default);
}
