using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NexaMart.Application.Interfaces.Services;
using NexaMart.Web.Extensions;
using NexaMart.Web.Mappings;
using NexaMart.Web.Models;

namespace NexaMart.Web.Controllers;

/// <summary>
/// Lean controller managing customer cart CRUD operations and checkout dispatching.
/// Delegates all inventory checks and business mutations to Application Services.
/// </summary>
[Authorize]
public class CartController : Controller
{
    private readonly ICartService _cartService;
    private readonly IOrderService _orderService;

    public CartController(ICartService cartService, IOrderService orderService)
    {
        _cartService = cartService;
        _orderService = orderService;
    }

    // =========================================================================
    // 1. VIEW CART
    // =========================================================================

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var cartItems = await _cartService.GetCartAsync(User.GetUserId(), HttpContext.RequestAborted);
        return View(cartItems.ToViewModel());
    }

    // =========================================================================
    // 2. ADD TO CART
    // =========================================================================

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddToCart(int productId, int quantity = 1, string? returnUrl = null)
    {
        try
        {
            await _cartService.AddToCartAsync(User.GetUserId(), productId, quantity, HttpContext.RequestAborted);
            TempData["SuccessMessage"] = "Product added to your cart successfully!";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }

        return RedirectToAction(nameof(Index));
    }

    // =========================================================================
    // 3. UPDATE QUANTITY (Increase / Decrease / Set)
    // =========================================================================

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateQuantity(int cartItemId, int quantity)
    {
        try
        {
            await _cartService.UpdateQuantityAsync(User.GetUserId(), cartItemId, quantity, HttpContext.RequestAborted);
            TempData["SuccessMessage"] = "Cart item quantity updated.";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    // =========================================================================
    // 4. REMOVE ITEM FROM CART
    // =========================================================================

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveFromCart(int cartItemId)
    {
        try
        {
            await _cartService.RemoveFromCartAsync(User.GetUserId(), cartItemId, HttpContext.RequestAborted);
            TempData["SuccessMessage"] = "Item removed from your cart.";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    // =========================================================================
    // 5. CLEAR ENTIRE CART
    // =========================================================================

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Clear()
    {
        await _cartService.ClearCartAsync(User.GetUserId(), HttpContext.RequestAborted);
        TempData["SuccessMessage"] = "Your cart has been cleared.";
        return RedirectToAction(nameof(Index));
    }

    // =========================================================================
    // 6. GENERATE OFFICIAL INVOICE DIRECTLY FROM CART
    // =========================================================================

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> GenerateInvoice()
    {
        try
        {
            var order = await _orderService.CreateOrderFromCartAsync(User.GetUserId(), HttpContext.RequestAborted);
            TempData["SuccessMessage"] = $"Official invoice #{order.OrderNumber} generated successfully!";
            return RedirectToAction("Invoice", "Orders", new { id = order.Id });
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            return RedirectToAction(nameof(Index));
        }
    }
}

