using NexaMart.Domain.Entities;

namespace NexaMart.Application.Interfaces.Services;

/// <summary>
/// Service contract managing verified product ratings and customer feedback reviews.
/// </summary>
public interface IReviewService
{
    Task<IReadOnlyList<Review>> GetProductReviewsAsync(int productId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Review>> GetUserReviewsAsync(int userId, CancellationToken cancellationToken = default);
    Task<Review?> GetUserProductReviewAsync(int userId, int productId, CancellationToken cancellationToken = default);
    Task<Review> AddOrUpdateReviewAsync(int userId, int productId, int rating, string? comment, CancellationToken cancellationToken = default);
    Task<bool> DeleteReviewAsync(int reviewId, int userId, bool isAdmin = false, CancellationToken cancellationToken = default);
    Task<(decimal AverageRating, int ReviewCount)> GetProductRatingSummaryAsync(int productId, CancellationToken cancellationToken = default);
}
