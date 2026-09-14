using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NexaMart.Application.Common.Models;
using NexaMart.Application.DTOs.Common;
using NexaMart.Application.Interfaces.Services;
using NexaMart.Web.Mappings;
using NexaMart.Web.Models;

namespace NexaMart.Web.Controllers;

/// <summary>
/// Lean controller delegating all category catalog queries, search rules,
/// and CRUD operations directly to ICategoryService.
/// </summary>
public class CategoriesController : Controller
{
    private readonly ICategoryService _categoryService;

    public CategoriesController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    // =========================================================================
    // 1. PUBLIC CATALOG & SEARCH (Name for all, ID strictly restricted in Service)
    // =========================================================================

    [HttpGet]
    public async Task<IActionResult> Index([FromQuery] CategoryFilterDto filter)
    {
        bool isStaff = User.IsInRole("Admin") || User.IsInRole("SuperAdmin");

        // Service enforces: search by ID restricted to staff, and public visitors only see active categories
        var pagedCategories = await _categoryService.GetCategoriesPagedAsync(filter, isStaff, HttpContext.RequestAborted);
        var model = pagedCategories.ToListViewModel(filter);

        ViewBag.IsStaff = isStaff;
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        bool isStaff = User.IsInRole("Admin") || User.IsInRole("SuperAdmin");

        // Service returns null if inactive and caller is not staff
        var category = await _categoryService.GetCategoryByIdAsync(id, isStaff, HttpContext.RequestAborted);
        if (category == null)
        {
            return NotFound();
        }

        ViewBag.IsStaff = isStaff;
        return View(category.ToCardViewModel());
    }

    // =========================================================================
    // 2. CREATE CATEGORY (Admin & SuperAdmin Only)
    // =========================================================================

    [Authorize(Policy = "StaffOnly")]
    [HttpGet]
    public IActionResult Create()
    {
        return View(new CategoryFormViewModel());
    }

    [Authorize(Policy = "StaffOnly")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CategoryFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var created = await _categoryService.CreateCategoryAsync(model.ToEntity(), HttpContext.RequestAborted);
        TempData["SuccessMessage"] = $"Category '{created.Name}' created successfully.";
        return RedirectToAction(nameof(Index));
    }

    // =========================================================================
    // 3. EDIT CATEGORY (Admin & SuperAdmin Only)
    // =========================================================================

    [Authorize(Policy = "StaffOnly")]
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var category = await _categoryService.GetCategoryByIdAsync(id, isStaff: true, HttpContext.RequestAborted);
        if (category == null)
        {
            return NotFound();
        }

        return View(category.ToFormViewModel());
    }

    [Authorize(Policy = "StaffOnly")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, CategoryFormViewModel model)
    {
        if (id != model.Id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var updated = await _categoryService.UpdateCategoryAsync(id, model.ToEntity(), HttpContext.RequestAborted);
        TempData["SuccessMessage"] = $"Category '{updated.Name}' updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    // =========================================================================
    // 4. DELETE CATEGORY (Admin & SuperAdmin Only)
    // =========================================================================

    [Authorize(Policy = "StaffOnly")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var success = await _categoryService.DeleteCategoryAsync(id, HttpContext.RequestAborted);
            if (!success)
            {
                return NotFound();
            }

            TempData["SuccessMessage"] = "Category deleted successfully.";
        }
        catch (InvalidOperationException ex)
        {
            // Business rule enforced by CategoryService: cannot delete category with products
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }
}
