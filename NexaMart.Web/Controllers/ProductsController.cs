using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using NexaMart.Application.Common.Models;
using NexaMart.Application.DTOs.Common;
using NexaMart.Application.Interfaces.Services;
using NexaMart.Web.Mappings;
using NexaMart.Web.Models;

namespace NexaMart.Web.Controllers;

/// <summary>
/// Lean controller delegating all catalog queries, search rules, product CRUD, 
/// and customer rating/review operations directly to Application Layer Services.
/// </summary>
public class ProductsController : Controller
{
    private readonly IProductService _productService;
    private readonly ICategoryService _categoryService;
    private readonly ICartService _cartService;
    private readonly IWishlistService _wishlistService;
    private readonly IReviewService _reviewService;

    public ProductsController(
        IProductService productService,
        ICategoryService categoryService,
        ICartService cartService,
        IWishlistService wishlistService,
        IReviewService reviewService)
    {
        _productService = productService;
        _categoryService = categoryService;
        _cartService = cartService;
        _wishlistService = wishlistService;
        _reviewService = reviewService;
    }

    // =========================================================================
    // 1. PUBLIC CATALOG & SEARCH (Name for all, ID strictly restricted in Service)
    // =========================================================================

    [HttpGet]
    public async Task<IActionResult> Index([FromQuery] ProductFilterDto filter)
    {
        bool isStaff = User.IsInRole("Admin") || User.IsInRole("SuperAdmin");

        // Service enforces: search by ID restricted to staff, and public visitors only see active items
        var pagedProducts = await _productService.GetProductsPagedAsync(filter, isStaff, HttpContext.RequestAborted);
        var activeCategories = await _categoryService.GetAllActiveCategoriesAsync(HttpContext.RequestAborted);

        var model = pagedProducts.ToListViewModel(filter, activeCategories);
        ViewBag.IsStaff = isStaff;
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        bool isStaff = User.IsInRole("Admin") || User.IsInRole("SuperAdmin");

        // Service returns null if inactive and caller is not staff
        var product = await _productService.GetProductByIdAsync(id, isStaff, HttpContext.RequestAborted);
        if (product == null)
        {
            return NotFound();
        }

        bool isInWishlist = false;
        int currentCartQuantity = 0;

        if (User.Identity?.IsAuthenticated == true &&
            int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
        {
            isInWishlist = await _wishlistService.IsInWishlistAsync(userId, id, HttpContext.RequestAborted);
            var cartItem = await _cartService.GetCartItemAsync(userId, id, HttpContext.RequestAborted);
            currentCartQuantity = cartItem?.Quantity ?? 0;
        }

        // Service calls for related products and reviews
        var relatedProducts = await _productService.GetRelatedProductsAsync(id, product.CategoryId, 4, HttpContext.RequestAborted);
        var reviews = await _reviewService.GetProductReviewsAsync(id, HttpContext.RequestAborted);

        var model = product.ToDetailsViewModel(isInWishlist, currentCartQuantity, relatedProducts, reviews);
        ViewBag.IsStaff = isStaff;

        return View(model);
    }

    // =========================================================================
    // 2. PRODUCT RATING & REVIEWS SYSTEM (Direct via IReviewService)
    // =========================================================================

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddReview(AddReviewViewModel model)
    {
        if (!ModelState.IsValid)
        {
            TempData["ErrorMessage"] = "Please select a star rating between 1 and 5.";
            return RedirectToAction(nameof(Details), new { id = model.ProductId });
        }

        try
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            await _reviewService.AddOrUpdateReviewAsync(
                userId,
                model.ProductId,
                model.Rating,
                model.Comment,
                HttpContext.RequestAborted);

            TempData["SuccessMessage"] = "Your review and rating have been posted successfully!";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToAction(nameof(Details), new { id = model.ProductId });
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteReview(int reviewId, int productId)
    {
        try
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            bool isAdmin = User.IsInRole("Admin") || User.IsInRole("SuperAdmin");

            await _reviewService.DeleteReviewAsync(reviewId, userId, isAdmin, HttpContext.RequestAborted);
            TempData["SuccessMessage"] = "Review deleted successfully.";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToAction(nameof(Details), new { id = productId });
    }

    // =========================================================================
    // 3. CREATE PRODUCT (Admin & SuperAdmin Only)
    // =========================================================================

    [Authorize(Policy = "StaffOnly")]
    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var model = new ProductFormViewModel
        {
            CategoriesList = await GetCategoriesSelectListAsync()
        };

        return View(model);
    }

    [Authorize(Policy = "StaffOnly")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ProductFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            model.CategoriesList = await GetCategoriesSelectListAsync(model.CategoryId);
            return View(model);
        }

        var created = await _productService.CreateProductAsync(model.ToEntity(), HttpContext.RequestAborted);
        TempData["SuccessMessage"] = $"Product '{created.Name}' created successfully.";
        return RedirectToAction(nameof(Details), new { id = created.Id });
    }

    // =========================================================================
    // 4. EDIT PRODUCT (Admin & SuperAdmin Only)
    // =========================================================================

    [Authorize(Policy = "StaffOnly")]
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var product = await _productService.GetProductByIdAsync(id, isStaff: true, HttpContext.RequestAborted);
        if (product == null)
        {
            return NotFound();
        }

        var categories = await GetCategoriesSelectListAsync(product.CategoryId);
        return View(product.ToFormViewModel(categories));
    }

    [Authorize(Policy = "StaffOnly")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ProductFormViewModel model)
    {
        if (id != model.Id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            model.CategoriesList = await GetCategoriesSelectListAsync(model.CategoryId);
            return View(model);
        }

        var updated = await _productService.UpdateProductAsync(id, model.ToEntity(), HttpContext.RequestAborted);
        TempData["SuccessMessage"] = $"Product '{updated.Name}' updated successfully.";
        return RedirectToAction(nameof(Details), new { id = updated.Id });
    }

    // =========================================================================
    // 5. DELETE PRODUCT (Admin & SuperAdmin Only)
    // =========================================================================

    [Authorize(Policy = "StaffOnly")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var success = await _productService.DeleteProductAsync(id, HttpContext.RequestAborted);
        if (!success)
        {
            return NotFound();
        }

        TempData["SuccessMessage"] = "Product deleted successfully.";
        return RedirectToAction(nameof(Index));
    }

    // =========================================================================
    // HELPER
    // =========================================================================

    private async Task<IEnumerable<SelectListItem>> GetCategoriesSelectListAsync(int? selectedId = null)
    {
        var categories = await _categoryService.GetAllActiveCategoriesAsync(HttpContext.RequestAborted);
        return categories.Select(c => new SelectListItem
        {
            Value = c.Id.ToString(),
            Text = c.Name,
            Selected = selectedId.HasValue && c.Id == selectedId.Value
        });
    }
}
