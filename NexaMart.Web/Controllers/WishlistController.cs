using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NexaMart.Application.Interfaces.Services;
using NexaMart.Web.Extensions;
using NexaMart.Web.Mappings;

namespace NexaMart.Web.Controllers;

/// <summary>
/// Lean controller managing customer saved wishlist items.
/// Fully delegates data manipulation to IWishlistService.
/// </summary>
[Authorize]
public class WishlistController : Controller
{
    private readonly IWishlistService _wishlistService;

    public WishlistController(IWishlistService wishlistService)
    {
        _wishlistService = wishlistService;
    }

    // =========================================================================
    // 1. VIEW WISHLIST
    // =========================================================================

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var items = await _wishlistService.GetUserWishlistAsync(User.GetUserId(), HttpContext.RequestAborted);
        return View(items.ToViewModel());
    }

    // =========================================================================
    // 2. TOGGLE / ADD TO WISHLIST
    // =========================================================================

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Toggle(int productId, string? returnUrl = null)
    {
        try
        {
            var added = await _wishlistService.ToggleWishlistAsync(User.GetUserId(), productId, HttpContext.RequestAborted);
            TempData["SuccessMessage"] = added 
                ? "Product added to your wishlist!" 
                : "Product removed from your wishlist.";
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
    // 3. REMOVE FROM WISHLIST
    // =========================================================================

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Remove(int productId)
    {
        try
        {
            await _wishlistService.RemoveFromWishlistAsync(User.GetUserId(), productId, HttpContext.RequestAborted);
            TempData["SuccessMessage"] = "Item removed from wishlist.";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    // =========================================================================
    // 4. MOVE ITEM FROM WISHLIST TO CART
    // =========================================================================

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MoveToCart(int productId)
    {
        try
        {
            await _wishlistService.MoveToCartAsync(User.GetUserId(), productId, HttpContext.RequestAborted);
            TempData["SuccessMessage"] = "Item moved to your shopping cart!";
            return RedirectToAction("Index", "Cart");
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            return RedirectToAction(nameof(Index));
        }
    }

    // =========================================================================
    // 5. CLEAR ENTIRE WISHLIST
    // =========================================================================

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Clear()
    {
        await _wishlistService.ClearWishlistAsync(User.GetUserId(), HttpContext.RequestAborted);
        TempData["SuccessMessage"] = "Your wishlist has been cleared.";
        return RedirectToAction(nameof(Index));
    }
}
