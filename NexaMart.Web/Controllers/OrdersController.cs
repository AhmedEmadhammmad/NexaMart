using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NexaMart.Application.Interfaces.Services;
using NexaMart.Web.Extensions;
using NexaMart.Web.Mappings;

namespace NexaMart.Web.Controllers;

/// <summary>
/// Lean controller managing customer order history, cancellation, and official invoice receipts.
/// </summary>
[Authorize]
public class OrdersController : Controller
{
    private readonly IOrderService _orderService;

    public OrdersController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    // =========================================================================
    // 1. CUSTOMER ORDER HISTORY
    // =========================================================================

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var orders = await _orderService.GetUserOrdersAsync(User.GetUserId(), HttpContext.RequestAborted);
        return View(orders.ToDetailsViewModelList());
    }

    // =========================================================================
    // 2. COMPLETE INVOICE & ORDER DETAILS
    // =========================================================================

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        return await GetOrderInvoiceAsync(id);
    }

    [HttpGet]
    public async Task<IActionResult> Invoice(int id)
    {
        return await GetOrderInvoiceAsync(id);
    }

    // =========================================================================
    // 3. CANCEL ORDER
    // =========================================================================

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(int id, string? reason = null)
    {
        var cancelReason = string.IsNullOrWhiteSpace(reason) ? "Customer requested cancellation" : reason;
        
        try
        {
            var success = await _orderService.CancelOrderAsync(id, cancelReason, User.GetUserId(), HttpContext.RequestAborted);
            if (success)
            {
                TempData["SuccessMessage"] = "Order has been cancelled successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = "Could not cancel this order. It may have already shipped or been delivered.";
            }
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToAction(nameof(Details), new { id });
    }

    // =========================================================================
    // HELPER: ORDER OWNERSHIP & INVOICE RESOLVER
    // =========================================================================

    private async Task<IActionResult> GetOrderInvoiceAsync(int orderId)
    {
        var order = await _orderService.GetOrderByIdAsync(orderId, HttpContext.RequestAborted);
        if (order == null)
        {
            TempData["ErrorMessage"] = "Order not found.";
            return RedirectToAction(nameof(Index));
        }

        // Authorization check: User can only view their own orders unless they are Staff
        bool isStaff = User.IsInRole("Admin") || User.IsInRole("SuperAdmin");
        if (order.UserId != User.GetUserId() && !isStaff)
        {
            return RedirectToAction("AccessDenied", "Account");
        }

        return View("Invoice", order.ToDetailsViewModel());
    }
}
