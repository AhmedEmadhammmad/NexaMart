using Microsoft.EntityFrameworkCore;
using NexaMart.Application.Interfaces.Repositories;
using NexaMart.Application.Interfaces.Services;
using NexaMart.Domain.Entities;

namespace NexaMart.Application.Services;

/// <summary>
/// Service managing verified product customer reviews, rating calculations, and feedback.
/// </summary>
public class ReviewService : IReviewService
{
    private readonly IUnitOfWork _unitOfWork;

    public ReviewService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<Review>> GetProductReviewsAsync(int productId, CancellationToken cancellationToken = default)
    {
        return await _unitOfWork.Reviews.Query()
            .Include(r => r.User)
            .Where(r => r.ProductId == productId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Review>> GetUserReviewsAsync(int userId, CancellationToken cancellationToken = default)
    {
        return await _unitOfWork.Reviews.Query()
            .Include(r => r.Product)
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<Review?> GetUserProductReviewAsync(int userId, int productId, CancellationToken cancellationToken = default)
    {
        return await _unitOfWork.Reviews.Query()
            .FirstOrDefaultAsync(r => r.UserId == userId && r.ProductId == productId, cancellationToken);
    }

    public async Task<Review> AddOrUpdateReviewAsync(int userId, int productId, int rating, string? comment, CancellationToken cancellationToken = default)
    {
        if (rating < 1 || rating > 5)
        {
            throw new ArgumentOutOfRangeException(nameof(rating), "Rating must be between 1 and 5 stars.");
        }

        var product = await _unitOfWork.Products.GetByIdAsync(productId, cancellationToken);
        if (product == null || !product.IsActive)
        {
            throw new KeyNotFoundException($"Product with ID {productId} was not found or is inactive.");
        }

        var existingReview = await _unitOfWork.Reviews.Query(disableTracking: false)
            .FirstOrDefaultAsync(r => r.UserId == userId && r.ProductId == productId, cancellationToken);

        Review review;

        if (existingReview != null)
        {
            existingReview.Rating = rating;
            existingReview.Comment = comment?.Trim();
            existingReview.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.Reviews.Update(existingReview);
            review = existingReview;
        }
        else
        {
            review = new Review
            {
                UserId = userId,
                ProductId = productId,
                Rating = rating,
                Comment = comment?.Trim(),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = null
            };
            await _unitOfWork.Reviews.AddAsync(review, cancellationToken);
        }

        await _unitOfWork.CompleteAsync(cancellationToken);

        // Recalculate product average rating and review count
        await UpdateProductRatingStatsAsync(productId, cancellationToken);

        return review;
    }

    public async Task<bool> DeleteReviewAsync(int reviewId, int userId, bool isAdmin = false, CancellationToken cancellationToken = default)
    {
        var review = await _unitOfWork.Reviews.GetByIdAsync(reviewId, cancellationToken);
        if (review == null)
        {
            return false;
        }

        if (!isAdmin && review.UserId != userId)
        {
            throw new UnauthorizedAccessException("You are not authorized to delete this review.");
        }

        int productId = review.ProductId;

        _unitOfWork.Reviews.Delete(review);
        await _unitOfWork.CompleteAsync(cancellationToken);

        // Recalculate product rating stats
        await UpdateProductRatingStatsAsync(productId, cancellationToken);

        return true;
    }

    public async Task<(decimal AverageRating, int ReviewCount)> GetProductRatingSummaryAsync(int productId, CancellationToken cancellationToken = default)
    {
        var reviews = await _unitOfWork.Reviews.Query()
            .Where(r => r.ProductId == productId)
            .Select(r => r.Rating)
            .ToListAsync(cancellationToken);

        if (reviews.Count == 0)
        {
            return (0.0m, 0);
        }

        var avg = Math.Round((decimal)reviews.Average(), 2);
        return (avg, reviews.Count);
    }

    private async Task UpdateProductRatingStatsAsync(int productId, CancellationToken cancellationToken)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(productId, cancellationToken);
        if (product == null) return;

        var stats = await GetProductRatingSummaryAsync(productId, cancellationToken);
        product.AverageRating = stats.AverageRating;
        product.ReviewCount = stats.ReviewCount;
        product.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Products.Update(product);
        await _unitOfWork.CompleteAsync(cancellationToken);
    }
}
