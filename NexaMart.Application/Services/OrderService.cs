using Microsoft.EntityFrameworkCore;
using NexaMart.Application.Common.Models;
using NexaMart.Application.Interfaces.Repositories;
using NexaMart.Application.Interfaces.Services;
using NexaMart.Domain.Entities;
using NexaMart.Domain.Enums;

namespace NexaMart.Application.Services;

/// <summary>
/// Service implementing order placement, inventory deduction, cancellation, and retrieval.
/// </summary>
public class OrderService : IOrderService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmailService _emailService;

    public OrderService(IUnitOfWork unitOfWork, IEmailService emailService)
    {
        _unitOfWork = unitOfWork;
        _emailService = emailService;
    }

    public async Task<Order> CreateOrderFromCartAsync(int userId, CancellationToken cancellationToken = default)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(userId, cancellationToken);
        var request = new CreateOrderRequest
        {
            UserId = userId,
            CustomerName = user?.FullName ?? "Customer",
            CustomerEmail = user?.Email ?? string.Empty,
            CustomerPhone = user?.PhoneNumber ?? string.Empty,
            ShippingAddress = "Default Delivery Address",
            City = "Cairo",
            State = "Cairo",
            PaymentMethod = PaymentMethod.CashOnDelivery,
            ShippingCost = 0.0m,
            TaxAmount = 0.0m,
            DiscountAmount = 0.0m
        };

        return await CreateOrderFromCartAsync(request, cancellationToken);
    }

    public async Task<Order> CreateOrderFromCartAsync(CreateOrderRequest request, CancellationToken cancellationToken = default)
    {
        // 1. Retrieve user's cart items with product details (tracking enabled to mutate inventory)
        var cartItems = await _unitOfWork.CartItems.Query(disableTracking: false)
            .Include(c => c.Product)
            .Where(c => c.UserId == request.UserId)
            .ToListAsync(cancellationToken);

        if (cartItems.Count == 0)
        {
            throw new InvalidOperationException("Cannot place an order with an empty shopping cart.");
        }

        // 2. Validate inventory availability for all items
        foreach (var item in cartItems)
        {
            if (item.Product.StockQuantity < item.Quantity)
            {
                throw new InvalidOperationException($"Product '{item.Product.Name}' has insufficient stock. Available: {item.Product.StockQuantity}, Requested: {item.Quantity}");
            }
        }

        // 3. Begin atomic database transaction
        await _unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            decimal subTotal = 0.0m;
            var orderItems = new List<OrderItem>();

            foreach (var item in cartItems)
            {
                var product = item.Product;
                if (product == null) continue;

                var lineTotal = product.Price * item.Quantity;

                // Deduct inventory
                product.StockQuantity -= item.Quantity;
                product.UpdatedAt = DateTime.UtcNow;
                _unitOfWork.Products.Update(product);

                subTotal += lineTotal;

                orderItems.Add(new OrderItem
                {
                    ProductId = product.Id,
                    ProductName = product.Name,
                    ProductImageUrl = product.ImageUrl,
                    UnitPrice = product.Price,
                    Quantity = item.Quantity,
                    TotalPrice = lineTotal,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = null
                });
            }

            var shippingCost = request.ShippingCost;
            var taxAmount = request.TaxAmount;
            var discountAmount = request.DiscountAmount;
            var totalAmount = Math.Max(0.0m, subTotal + shippingCost + taxAmount - discountAmount);

            var order = new Order
            {
                OrderNumber = $"NXM-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..6].ToUpperInvariant()}",
                UserId = request.UserId,
                SubTotal = subTotal,
                ShippingCost = shippingCost,
                TaxAmount = taxAmount,
                DiscountAmount = discountAmount,
                TotalAmount = totalAmount,
                Currency = "EGP",
                PaymentMethod = request.PaymentMethod,
                PaymentStatus = PaymentStatus.Pending,
                CustomerName = request.CustomerName,
                CustomerEmail = request.CustomerEmail,
                CustomerPhone = request.CustomerPhone,
                ShippingAddress = request.ShippingAddress,
                City = request.City,
                State = request.State,
                PostalCode = request.PostalCode,
                OrderNotes = request.OrderNotes,
                Status = request.PaymentMethod == PaymentMethod.CashOnDelivery 
                    ? OrderStatus.Confirmed 
                    : OrderStatus.Pending,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = null,
                OrderItems = orderItems
            };

            await _unitOfWork.Orders.AddAsync(order, cancellationToken);

            // Clear shopping cart items (using the already tracked cartItems in memory)
            _unitOfWork.CartItems.DeleteRange(cartItems);

            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            var createdOrder = (await GetOrderByIdAsync(order.Id, cancellationToken))!;

            // For CashOnDelivery: Order is instantly confirmed -> send official invoice email immediately.
            // For Paymob Online payments: Email is deferred until payment is captured and verified.
            if (request.PaymentMethod == PaymentMethod.CashOnDelivery)
            {
                await _emailService.SendOrderConfirmationEmailAsync(createdOrder, cancellationToken);
            }

            return createdOrder;
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }

    public async Task<PagedResult<Order>> GetOrdersPagedAsync(
        int? userId = null,
        OrderStatus? status = null,
        string? searchTerm = null,
        int? searchId = null,
        int pageIndex = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var query = _unitOfWork.Orders.Query()
            .Include(o => o.User)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
            .AsQueryable();

        // 1. Search strictly by Order ID
        if (searchId.HasValue)
        {
            query = query.Where(o => o.Id == searchId.Value);
        }

        // 2. Keyword search (OrderNumber or User details)
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.Trim().ToLower();
            query = query.Where(o => o.OrderNumber.ToLower().Contains(term) ||
                                     o.User.FullName.ToLower().Contains(term) ||
                                     o.User.Email.ToLower().Contains(term));
        }

        // 3. User filter
        if (userId.HasValue)
        {
            query = query.Where(o => o.UserId == userId.Value);
        }

        // 4. Order status filter
        if (status.HasValue)
        {
            query = query.Where(o => o.Status == status.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        pageIndex = pageIndex < 1 ? 1 : pageIndex;
        pageSize = pageSize < 1 ? 10 : pageSize;

        var items = await query
            .OrderByDescending(o => o.CreatedAt)
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<Order>(items, totalCount, pageIndex, pageSize);
    }

    public async Task<IReadOnlyList<Order>> GetUserOrdersAsync(int userId, CancellationToken cancellationToken = default)
    {
        return await _unitOfWork.Orders.Query()
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<Order?> GetOrderByIdAsync(int orderId, CancellationToken cancellationToken = default)
    {
        return await _unitOfWork.Orders.Query()
            .Include(o => o.User)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
            .FirstOrDefaultAsync(o => o.Id == orderId, cancellationToken);
    }

    public async Task<Order?> GetOrderByNumberAsync(string orderNumber, CancellationToken cancellationToken = default)
    {
        var normalized = orderNumber.Trim().ToLower();
        return await _unitOfWork.Orders.Query()
            .Include(o => o.User)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
            .FirstOrDefaultAsync(o => o.OrderNumber.ToLower() == normalized, cancellationToken);
    }

    public async Task<bool> UpdateOrderStatusAsync(int orderId, OrderStatus status, CancellationToken cancellationToken = default)
    {
        var order = await _unitOfWork.Orders.GetByIdAsync(orderId, cancellationToken);
        if (order == null) return false;

        order.Status = status;
        order.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Orders.Update(order);
        await _unitOfWork.CompleteAsync(cancellationToken);
        return true;
    }

    public async Task<bool> CancelOrderAsync(int orderId, string reason, int cancelledByUserId, CancellationToken cancellationToken = default)
    {
        var order = await _unitOfWork.Orders.Query()
            .Include(o => o.OrderItems)
            .FirstOrDefaultAsync(o => o.Id == orderId, cancellationToken);

        if (order == null || order.Status == OrderStatus.Cancelled || order.Status == OrderStatus.Delivered)
        {
            return false;
        }

        await _unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            order.Status = OrderStatus.Cancelled;
            order.CancellationReason = reason;
            order.CancelledAt = DateTime.UtcNow;
            order.CancelledByUserId = cancelledByUserId;
            order.UpdatedAt = DateTime.UtcNow;

            // Restore product stock
            foreach (var item in order.OrderItems)
            {
                var product = await _unitOfWork.Products.GetByIdAsync(item.ProductId, cancellationToken);
                if (product != null)
                {
                    product.StockQuantity += item.Quantity;
                    product.UpdatedAt = DateTime.UtcNow;
                    _unitOfWork.Products.Update(product);
                }
                item.IsCancelled = true;
                item.UpdatedAt = DateTime.UtcNow;
            }

            _unitOfWork.Orders.Update(order);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            // Dispatch cancellation confirmation email to customer
            var fullCancelledOrder = await GetOrderByIdAsync(orderId, cancellationToken);
            if (fullCancelledOrder != null)
            {
                await _emailService.SendOrderCancellationEmailAsync(fullCancelledOrder, reason, cancellationToken);
            }

            return true;
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }

    public async Task<(int TotalOrders, decimal TotalRevenue)> GetOrderStatsAsync(CancellationToken cancellationToken = default)
    {
        var query = _unitOfWork.Orders.Query().AsNoTracking();
        var totalOrders = await query.CountAsync(cancellationToken);
        var totalRevenue = totalOrders > 0 
            ? await query.Where(o => o.Status != OrderStatus.Cancelled).SumAsync(o => o.TotalAmount, cancellationToken)
            : 0.00m;

        return (totalOrders, totalRevenue);
    }

    public async Task<bool> UpdatePaymentStatusAsync(int orderId, PaymentStatus status, string? transactionId = null, CancellationToken cancellationToken = default)
    {
        var order = await _unitOfWork.Orders.GetByIdAsync(orderId, cancellationToken);
        if (order == null)
        {
            return false;
        }

        var wasAlreadyPaid = order.PaymentStatus == PaymentStatus.Paid;
        order.PaymentStatus = status;

        if (status == PaymentStatus.Paid)
        {
            order.Status = OrderStatus.Confirmed;
            order.PaidAt = DateTime.UtcNow;
        }

        if (!string.IsNullOrWhiteSpace(transactionId))
        {
            order.TransactionId = transactionId;
        }

        order.UpdatedAt = DateTime.UtcNow;
        _unitOfWork.Orders.Update(order);
        await _unitOfWork.CompleteAsync(cancellationToken);

        // Dispatch official invoice confirmation email when payment is successfully captured
        if (status == PaymentStatus.Paid && !wasAlreadyPaid)
        {
            var fullOrder = await GetOrderByIdAsync(orderId, cancellationToken);
            if (fullOrder != null)
            {
                await _emailService.SendOrderConfirmationEmailAsync(fullOrder, cancellationToken);
            }
        }

        return true;
    }
}
