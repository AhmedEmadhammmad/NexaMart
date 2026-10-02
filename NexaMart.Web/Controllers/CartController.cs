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
    private readonly IUserService _userService;
    private readonly IPaymobService _paymobService;

    public CartController(
        ICartService cartService, 
        IOrderService orderService,
        IUserService userService,
        IPaymobService paymobService)
    {
        _cartService = cartService;
        _orderService = orderService;
        _userService = userService;
        _paymobService = paymobService;
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
    // 6. CHECKOUT WITH PAYMENT SELECTION & BILLING (Paymob Ready)
    // =========================================================================

    [HttpGet]
    public async Task<IActionResult> Checkout()
    {
        var userId = User.GetUserId();
        var cartItems = await _cartService.GetCartAsync(userId, HttpContext.RequestAborted);
        if (!cartItems.Any())
        {
            TempData["ErrorMessage"] = "Your cart is empty. Please add items before checking out.";
            return RedirectToAction(nameof(Index));
        }

        var user = await _userService.GetProfileAsync(userId, HttpContext.RequestAborted);
        var cartViewModel = cartItems.ToViewModel();

        var model = new CheckoutViewModel
        {
            CustomerName = user.FullName,
            CustomerEmail = user.Email,
            CustomerPhone = user.PhoneNumber ?? string.Empty,
            ShippingAddress = "123 Nile Corniche Street",
            City = "Cairo",
            State = "Cairo",
            PaymentMethod = NexaMart.Domain.Enums.PaymentMethod.CashOnDelivery,
            SubTotal = cartViewModel.SubTotal,
            ShippingCost = cartViewModel.ShippingFee,
            TaxAmount = 0.0m,
            DiscountAmount = 0.0m,
            Currency = "EGP",
            CartItems = cartViewModel.Items
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Checkout(CheckoutViewModel model)
    {
        var userId = User.GetUserId();
        var cartItems = await _cartService.GetCartAsync(userId, HttpContext.RequestAborted);
        if (!cartItems.Any())
        {
            TempData["ErrorMessage"] = "Your cart is empty.";
            return RedirectToAction(nameof(Index));
        }

        if (!ModelState.IsValid)
        {
            var cartViewModel = cartItems.ToViewModel();
            model.CartItems = cartViewModel.Items;
            model.SubTotal = cartViewModel.SubTotal;
            model.ShippingCost = cartViewModel.ShippingFee;
            return View(model);
        }

        try
        {
            var request = new NexaMart.Application.Common.Models.CreateOrderRequest
            {
                UserId = userId,
                CustomerName = model.CustomerName,
                CustomerEmail = model.CustomerEmail,
                CustomerPhone = model.CustomerPhone,
                ShippingAddress = model.ShippingAddress,
                City = model.City,
                State = model.State,
                PostalCode = model.PostalCode,
                OrderNotes = model.OrderNotes,
                PaymentMethod = model.PaymentMethod,
                ShippingCost = model.ShippingCost,
                TaxAmount = model.TaxAmount,
                DiscountAmount = model.DiscountAmount
            };

            var order = await _orderService.CreateOrderFromCartAsync(request, HttpContext.RequestAborted);

            // If Paymob Online Card or Wallet is selected, initiate session and redirect customer to Iframe
            if (model.PaymentMethod == NexaMart.Domain.Enums.PaymentMethod.PaymobCard ||
                model.PaymentMethod == NexaMart.Domain.Enums.PaymentMethod.PaymobWallet)
            {
                var iframeUrl = await _paymobService.InitiatePaymentAsync(order, model.PaymentMethod, HttpContext.RequestAborted);
                return Redirect(iframeUrl);
            }

            // Cash On Delivery flow: instant confirmation and invoice redirection
            TempData["SuccessMessage"] = $"Order #{order.OrderNumber} placed successfully! An official invoice and confirmation email has been dispatched.";
            return RedirectToAction("Invoice", "Orders", new { id = order.Id });
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            var cartViewModel = cartItems.ToViewModel();
            model.CartItems = cartViewModel.Items;
            model.SubTotal = cartViewModel.SubTotal;
            model.ShippingCost = cartViewModel.ShippingFee;
            return View(model);
        }
    }

    // =========================================================================
    // 7. GENERATE OFFICIAL INVOICE DIRECTLY FROM CART (Quick Checkout)
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

