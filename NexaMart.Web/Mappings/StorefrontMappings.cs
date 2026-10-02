using NexaMart.Application.Common.Models;
using NexaMart.Application.DTOs.Common;
using NexaMart.Domain.Entities;
using NexaMart.Domain.Enums;
using NexaMart.Web.Models;

namespace NexaMart.Web.Mappings;

/// <summary>
/// Storefront mapping extensions for Customer and Public Catalog views.
/// </summary>
public static partial class MappingExtensions
{
    public static ProductCardViewModel ToCardViewModel(this Product product)
    {
        return new ProductCardViewModel
        {
            Id = product.Id,
            Name = product.Name,
            Price = product.Price,
            ImageUrl = product.ImageUrl,
            CategoryId = product.CategoryId,
            CategoryName = product.Category?.Name ?? string.Empty,
            AverageRating = product.AverageRating,
            ReviewCount = product.ReviewCount,
            StockQuantity = product.StockQuantity,
            IsActive = product.IsActive
        };
    }

    public static ProductDetailsViewModel ToDetailsViewModel(
        this Product product,
        bool isInWishlist = false,
        int currentCartQuantity = 0,
        IReadOnlyList<Product>? relatedProducts = null,
        IReadOnlyList<Review>? reviews = null)
    {
        return new ProductDetailsViewModel
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            ImageUrl = product.ImageUrl,
            CategoryId = product.CategoryId,
            CategoryName = product.Category?.Name ?? string.Empty,
            AverageRating = product.AverageRating,
            ReviewCount = product.ReviewCount,
            StockQuantity = product.StockQuantity,
            IsActive = product.IsActive,
            IsInWishlist = isInWishlist,
            CurrentCartQuantity = currentCartQuantity,
            RelatedProducts = relatedProducts?.Select(p => p.ToCardViewModel()).ToList() ?? new List<ProductCardViewModel>(),
            Reviews = reviews?.Select(r => r.ToViewModel()).ToList() ?? new List<ReviewItemViewModel>(),
            NewReview = new AddReviewViewModel { ProductId = product.Id }
        };
    }

    public static ProductListViewModel ToListViewModel(
        this PagedResult<Product> pagedProducts,
        ProductFilterDto filter,
        IReadOnlyList<Category> categories)
    {
        return new ProductListViewModel
        {
            Products = new PagedResult<ProductCardViewModel>(
                pagedProducts.Items.Select(p => p.ToCardViewModel()).ToList(),
                pagedProducts.TotalCount,
                pagedProducts.PageIndex,
                pagedProducts.PageSize),
            Filter = filter,
            Categories = categories.Select(c => c.ToCardViewModel()).ToList()
        };
    }

    public static CategoryListViewModel ToListViewModel(
        this PagedResult<Category> pagedCategories,
        CategoryFilterDto filter)
    {
        return new CategoryListViewModel
        {
            Categories = new PagedResult<CategoryCardViewModel>(
                pagedCategories.Items.Select(c => c.ToCardViewModel()).ToList(),
                pagedCategories.TotalCount,
                pagedCategories.PageIndex,
                pagedCategories.PageSize),
            Filter = filter
        };
    }

    public static CategoryCardViewModel ToCardViewModel(this Category category)
    {
        return new CategoryCardViewModel
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description,
            ImageUrl = category.ImageUrl,
            DisplayOrder = category.DisplayOrder,
            IsActive = category.IsActive,
            ProductCount = category.Products?.Count ?? 0
        };
    }

    public static CartItemViewModel ToViewModel(this CartItem item)
    {
        return new CartItemViewModel
        {
            Id = item.Id,
            ProductId = item.ProductId,
            ProductName = item.Product?.Name ?? string.Empty,
            ProductImageUrl = item.Product?.ImageUrl,
            UnitPrice = item.Product?.Price ?? 0.00m,
            Quantity = item.Quantity,
            AvailableStock = item.Product?.StockQuantity ?? 0
        };
    }

    public static CartViewModel ToViewModel(this IEnumerable<CartItem> items)
    {
        return new CartViewModel
        {
            Items = items.Select(i => i.ToViewModel()).ToList()
        };
    }

    public static WishlistItemViewModel ToViewModel(this WishlistItem item)
    {
        return new WishlistItemViewModel
        {
            Id = item.Id,
            ProductId = item.ProductId,
            ProductName = item.Product?.Name ?? string.Empty,
            ProductImageUrl = item.Product?.ImageUrl,
            Price = item.Product?.Price ?? 0.00m,
            CategoryName = item.Product?.Category?.Name ?? string.Empty,
            InStock = (item.Product?.StockQuantity ?? 0) > 0,
            AvailableStock = item.Product?.StockQuantity ?? 0
        };
    }

    public static WishlistViewModel ToViewModel(this IEnumerable<WishlistItem> items)
    {
        return new WishlistViewModel
        {
            Items = items.Select(i => i.ToViewModel()).ToList()
        };
    }

    public static OrderItemViewModel ToViewModel(this OrderItem item)
    {
        return new OrderItemViewModel
        {
            Id = item.Id,
            ProductId = item.ProductId,
            ProductName = item.ProductName,
            ProductImageUrl = item.ProductImageUrl,
            UnitPrice = item.UnitPrice,
            Quantity = item.Quantity,
            TotalPrice = item.TotalPrice,
            IsCancelled = item.IsCancelled
        };
    }

    public static OrderDetailsViewModel ToDetailsViewModel(this Order order)
    {
        return new OrderDetailsViewModel
        {
            Id = order.Id,
            OrderNumber = order.OrderNumber,
            UserId = order.UserId,
            UserName = !string.IsNullOrWhiteSpace(order.CustomerName) ? order.CustomerName : (order.User?.FullName ?? string.Empty),
            UserEmail = !string.IsNullOrWhiteSpace(order.CustomerEmail) ? order.CustomerEmail : (order.User?.Email ?? string.Empty),
            UserPhone = !string.IsNullOrWhiteSpace(order.CustomerPhone) ? order.CustomerPhone : order.User?.PhoneNumber,
            CustomerName = order.CustomerName,
            CustomerEmail = order.CustomerEmail,
            CustomerPhone = order.CustomerPhone,
            ShippingAddress = order.ShippingAddress,
            City = order.City,
            State = order.State,
            PostalCode = order.PostalCode,
            OrderNotes = order.OrderNotes,
            PaymentMethod = order.PaymentMethod,
            PaymentStatus = order.PaymentStatus,
            TransactionId = order.TransactionId,
            PaidAt = order.PaidAt,
            OrderDate = order.CreatedAt,
            Status = order.Status,
            SubTotal = order.SubTotal,
            ShippingCost = order.ShippingCost,
            TaxAmount = order.TaxAmount,
            DiscountAmount = order.DiscountAmount,
            TotalAmount = order.TotalAmount,
            Currency = order.Currency,
            CancellationReason = order.CancellationReason,
            CancelledAt = order.CancelledAt,
            OrderItems = order.OrderItems.Select(oi => oi.ToViewModel()).ToList()
        };
    }

    public static IReadOnlyList<OrderDetailsViewModel> ToDetailsViewModelList(this IEnumerable<Order> orders)
    {
        return orders.Select(o => o.ToDetailsViewModel()).ToList();
    }

    public static OrderListViewModel ToListViewModel(this PagedResult<Order> paged, OrderStatus? statusFilter = null)
    {
        return new OrderListViewModel
        {
            Orders = new PagedResult<OrderDetailsViewModel>(
                paged.Items.Select(o => o.ToDetailsViewModel()).ToList(),
                paged.TotalCount,
                paged.PageIndex,
                paged.PageSize),
            StatusFilter = statusFilter
        };
    }

    public static ReviewItemViewModel ToViewModel(this Review review)
    {
        return new ReviewItemViewModel
        {
            Id = review.Id,
            UserId = review.UserId,
            UserName = review.User?.FullName ?? "Customer",
            ProductId = review.ProductId,
            ProductName = review.Product?.Name ?? string.Empty,
            Rating = review.Rating,
            Comment = review.Comment,
            CreatedAt = review.CreatedAt
        };
    }

    public static ProfileViewModel ToProfileViewModel(this ApplicationUser user)
    {
        return new ProfileViewModel
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            RoleType = user.RoleType,
            CreatedAt = user.CreatedAt
        };
    }
}
