using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using NexaMart.Application.DTOs.Common;
using NexaMart.Application.Interfaces.Services;
using NexaMart.Domain.Enums;
using NexaMart.Web.Mappings;
using NexaMart.Web.Models;

namespace NexaMart.Web.Controllers;

/// <summary>
/// Razor-thin Store Administration Controller.
/// All business logic, multi-repository orchestration, and calculations
/// are strictly encapsulated in IAdminService.
/// </summary>
[Authorize(Policy = "StaffOnly")]
public class AdminController : Controller
{
    private readonly IAdminService _adminService;
    private readonly IFileStorageService _fileStorageService;

    public AdminController(IAdminService adminService, IFileStorageService fileStorageService)
    {
        _adminService = adminService;
        _fileStorageService = fileStorageService;
    }

    // =========================================================================
    // 1. DASHBOARD HUB
    // =========================================================================

    [HttpGet]
    public async Task<IActionResult> Dashboard(CancellationToken cancellationToken)
    {
        var dashboardDto = await _adminService.GetDashboardAsync(cancellationToken);
        return View(dashboardDto.ToViewModel());
    }

    // =========================================================================
    // 2. UNIVERSAL ID SEARCH
    // =========================================================================

    [HttpGet]
    [ActionName("SearchById")]
    public async Task<IActionResult> SearchById(int? id, string? type, CancellationToken cancellationToken)
    {
        if (!id.HasValue || id.Value <= 0)
        {
            TempData["ErrorMessage"] = "Please enter a valid numeric ID to search.";
            return RedirectToAction(nameof(Dashboard));
        }

        var result = await _adminService.ResolveSearchByIdAsync(id.Value, type, cancellationToken);
        if (!string.IsNullOrEmpty(result.ErrorMessage))
        {
            TempData["ErrorMessage"] = result.ErrorMessage;
        }

        return RedirectToAction(result.RouteAction, result.RouteController, result.RouteValues);
    }

    // =========================================================================
    // 3. PRODUCTS TABULAR MANAGEMENT (Full CRUD)
    // =========================================================================

    [HttpGet]
    public async Task<IActionResult> Products([FromQuery] ProductFilterDto filter, CancellationToken cancellationToken)
    {
        filter.PageSize = 15;
        var pagedProducts = await _adminService.GetProductsPagedAsync(filter, cancellationToken);
        var categories = await _adminService.GetActiveCategoriesAsync(cancellationToken);

        return View(pagedProducts.ToAdminProductListViewModel(filter, categories));
    }

    [HttpGet]
    public async Task<IActionResult> CreateProduct(CancellationToken cancellationToken)
    {
        var model = new ProductFormViewModel
        {
            CategoriesList = await GetCategorySelectListAsync(cancellationToken)
        };
        return View("ProductForm", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateProduct(ProductFormViewModel model, CancellationToken cancellationToken)
    {
        if (model.ImageFile != null && model.ImageFile.Length > 0)
        {
            try
            {
                using var stream = model.ImageFile.OpenReadStream();
                model.ImageUrl = await _fileStorageService.SaveImageAsync(stream, model.ImageFile.FileName, "products", cancellationToken);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(nameof(model.ImageFile), ex.Message);
            }
        }

        if (!ModelState.IsValid)
        {
            model.CategoriesList = await GetCategorySelectListAsync(cancellationToken, model.CategoryId);
            return View("ProductForm", model);
        }

        var created = await _adminService.CreateProductAsync(model.ToEntity(), cancellationToken);
        TempData["SuccessMessage"] = $"Product '{created.Name}' was successfully created with ID #{created.Id}.";
        return RedirectToAction(nameof(Products));
    }

    [HttpGet]
    public async Task<IActionResult> EditProduct(int id, CancellationToken cancellationToken)
    {
        var product = await _adminService.GetProductByIdAsync(id, cancellationToken);
        if (product == null)
        {
            TempData["ErrorMessage"] = $"Product #{id} was not found.";
            return RedirectToAction(nameof(Products));
        }

        var categoriesList = await GetCategorySelectListAsync(cancellationToken, product.CategoryId);
        return View("ProductForm", product.ToFormViewModel(categoriesList));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditProduct(int id, ProductFormViewModel model, CancellationToken cancellationToken)
    {
        string? oldImageUrl = null;
        if (model.ImageFile != null && model.ImageFile.Length > 0)
        {
            try
            {
                var existingProduct = await _adminService.GetProductByIdAsync(id, cancellationToken);
                oldImageUrl = existingProduct?.ImageUrl;

                using var stream = model.ImageFile.OpenReadStream();
                model.ImageUrl = await _fileStorageService.SaveImageAsync(stream, model.ImageFile.FileName, "products", cancellationToken);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(nameof(model.ImageFile), ex.Message);
            }
        }

        if (!ModelState.IsValid)
        {
            model.CategoriesList = await GetCategorySelectListAsync(cancellationToken, model.CategoryId);
            return View("ProductForm", model);
        }

        await _adminService.UpdateProductAsync(id, model.ToEntity(), cancellationToken);

        if (!string.IsNullOrEmpty(oldImageUrl) && oldImageUrl != model.ImageUrl)
        {
            await _fileStorageService.DeleteImageAsync(oldImageUrl);
        }

        TempData["SuccessMessage"] = $"Product #{id} was updated successfully.";
        return RedirectToAction(nameof(Products));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleProductStatus(int id, CancellationToken cancellationToken)
    {
        var isNowActive = await _adminService.ToggleProductStatusAsync(id, cancellationToken);
        TempData["SuccessMessage"] = $"Product #{id} status changed to {(isNowActive ? "Active" : "Inactive")}.";
        return RedirectToAction(nameof(Products));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteProduct(int id, CancellationToken cancellationToken)
    {
        var product = await _adminService.GetProductByIdAsync(id, cancellationToken);
        var imageUrl = product?.ImageUrl;

        var deleted = await _adminService.DeleteProductAsync(id, cancellationToken);
        if (deleted)
        {
            if (!string.IsNullOrEmpty(imageUrl))
            {
                await _fileStorageService.DeleteImageAsync(imageUrl);
            }
            TempData["SuccessMessage"] = $"Product #{id} was permanently removed.";
        }
        else
        {
            TempData["ErrorMessage"] = $"Product #{id} could not be deleted.";
        }

        return RedirectToAction(nameof(Products));
    }

    // =========================================================================
    // 4. CATEGORIES TABULAR MANAGEMENT (Full CRUD)
    // =========================================================================

    [HttpGet]
    public async Task<IActionResult> Categories([FromQuery] CategoryFilterDto filter, CancellationToken cancellationToken)
    {
        filter.PageSize = 15;
        var pagedCategories = await _adminService.GetCategoriesPagedAsync(filter, cancellationToken);
        return View(pagedCategories.ToAdminCategoryListViewModel(filter));
    }

    [HttpGet]
    public async Task<IActionResult> CreateCategory()
    {
        return View("CategoryForm", new CategoryFormViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateCategory(CategoryFormViewModel model, CancellationToken cancellationToken)
    {
        if (model.ImageFile != null && model.ImageFile.Length > 0)
        {
            try
            {
                using var stream = model.ImageFile.OpenReadStream();
                model.ImageUrl = await _fileStorageService.SaveImageAsync(stream, model.ImageFile.FileName, "categories", cancellationToken);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(nameof(model.ImageFile), ex.Message);
            }
        }

        if (!ModelState.IsValid)
        {
            return View("CategoryForm", model);
        }

        var created = await _adminService.CreateCategoryAsync(model.ToEntity(), cancellationToken);
        TempData["SuccessMessage"] = $"Category '{created.Name}' was successfully created with ID #{created.Id}.";
        return RedirectToAction(nameof(Categories));
    }

    [HttpGet]
    public async Task<IActionResult> EditCategory(int id, CancellationToken cancellationToken)
    {
        var category = await _adminService.GetCategoryByIdAsync(id, cancellationToken);
        if (category == null)
        {
            TempData["ErrorMessage"] = $"Category #{id} was not found.";
            return RedirectToAction(nameof(Categories));
        }

        return View("CategoryForm", category.ToFormViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditCategory(int id, CategoryFormViewModel model, CancellationToken cancellationToken)
    {
        string? oldImageUrl = null;
        if (model.ImageFile != null && model.ImageFile.Length > 0)
        {
            try
            {
                var existingCategory = await _adminService.GetCategoryByIdAsync(id, cancellationToken);
                oldImageUrl = existingCategory?.ImageUrl;

                using var stream = model.ImageFile.OpenReadStream();
                model.ImageUrl = await _fileStorageService.SaveImageAsync(stream, model.ImageFile.FileName, "categories", cancellationToken);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(nameof(model.ImageFile), ex.Message);
            }
        }

        if (!ModelState.IsValid)
        {
            return View("CategoryForm", model);
        }

        await _adminService.UpdateCategoryAsync(id, model.ToEntity(), cancellationToken);

        if (!string.IsNullOrEmpty(oldImageUrl) && oldImageUrl != model.ImageUrl)
        {
            await _fileStorageService.DeleteImageAsync(oldImageUrl);
        }

        TempData["SuccessMessage"] = $"Category #{id} was updated successfully.";
        return RedirectToAction(nameof(Categories));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleCategoryStatus(int id, CancellationToken cancellationToken)
    {
        var isNowActive = await _adminService.ToggleCategoryStatusAsync(id, cancellationToken);
        TempData["SuccessMessage"] = $"Category #{id} status changed to {(isNowActive ? "Active" : "Inactive")}.";
        return RedirectToAction(nameof(Categories));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteCategory(int id, CancellationToken cancellationToken)
    {
        try
        {
            var category = await _adminService.GetCategoryByIdAsync(id, cancellationToken);
            var imageUrl = category?.ImageUrl;

            var deleted = await _adminService.DeleteCategoryAsync(id, cancellationToken);
            if (deleted)
            {
                if (!string.IsNullOrEmpty(imageUrl))
                {
                    await _fileStorageService.DeleteImageAsync(imageUrl);
                }
                TempData["SuccessMessage"] = $"Category #{id} was deleted successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = $"Category #{id} could not be deleted.";
            }
        }
        catch (InvalidOperationException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToAction(nameof(Categories));
    }

    // =========================================================================
    // 5. ORDERS MANAGEMENT
    // =========================================================================

    [HttpGet]
    public async Task<IActionResult> Orders(
        OrderStatus? status,
        int? searchId,
        string? searchTerm,
        int page = 1,
        CancellationToken cancellationToken = default)
    {
        var pagedOrders = await _adminService.GetOrdersPagedAsync(
            status: status,
            searchId: searchId,
            searchTerm: searchTerm,
            pageIndex: page,
            pageSize: 15,
            cancellationToken: cancellationToken);

        return View(pagedOrders.ToAdminOrderListViewModel(status, searchId, searchTerm));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateOrderStatus(int orderId, OrderStatus status, CancellationToken cancellationToken)
    {
        var updated = await _adminService.UpdateOrderStatusAsync(orderId, status, cancellationToken);
        if (updated)
        {
            TempData["SuccessMessage"] = $"Order #{orderId} status successfully updated to '{status}'.";
        }
        else
        {
            TempData["ErrorMessage"] = $"Order #{orderId} status could not be updated.";
        }

        return RedirectToAction(nameof(Orders));
    }

    // =========================================================================
    // PRIVATE HELPERS
    // =========================================================================

    private async Task<IEnumerable<SelectListItem>> GetCategorySelectListAsync(CancellationToken cancellationToken, int? selectedId = null)
    {
        var categories = await _adminService.GetActiveCategoriesAsync(cancellationToken);
        return categories.Select(c => new SelectListItem
        {
            Value = c.Id.ToString(),
            Text = c.Name,
            Selected = selectedId.HasValue && c.Id == selectedId.Value
        });
    }
}
