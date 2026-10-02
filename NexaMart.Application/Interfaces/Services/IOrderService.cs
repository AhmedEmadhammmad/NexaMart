using NexaMart.Application.Common.Models;
using NexaMart.Domain.Entities;
using NexaMart.Domain.Enums;

namespace NexaMart.Application.Interfaces.Services;

/// <summary>
/// Service contract for order placement, lifecycle management, and querying.
/// </summary>
public interface IOrderService
{
    Task<Order> CreateOrderFromCartAsync(CreateOrderRequest request, CancellationToken cancellationToken = default);
    Task<Order> CreateOrderFromCartAsync(int userId, CancellationToken cancellationToken = default);
    Task<PagedResult<Order>> GetOrdersPagedAsync(int? userId = null, OrderStatus? status = null, string? searchTerm = null, int? searchId = null, int pageIndex = 1, int pageSize = 10, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Order>> GetUserOrdersAsync(int userId, CancellationToken cancellationToken = default);
    Task<Order?> GetOrderByIdAsync(int orderId, CancellationToken cancellationToken = default);
    Task<Order?> GetOrderByNumberAsync(string orderNumber, CancellationToken cancellationToken = default);
    Task<bool> UpdateOrderStatusAsync(int orderId, OrderStatus status, CancellationToken cancellationToken = default);
    Task<bool> UpdatePaymentStatusAsync(int orderId, PaymentStatus status, string? transactionId = null, CancellationToken cancellationToken = default);
    Task<bool> CancelOrderAsync(int orderId, string reason, int cancelledByUserId, CancellationToken cancellationToken = default);
    Task<(int TotalOrders, decimal TotalRevenue)> GetOrderStatsAsync(CancellationToken cancellationToken = default);
}
