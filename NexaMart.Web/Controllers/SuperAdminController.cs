using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NexaMart.Application.Interfaces.Services;
using NexaMart.Domain.Enums;
using NexaMart.Web.Extensions;
using NexaMart.Web.Mappings;
using NexaMart.Web.Models;

namespace NexaMart.Web.Controllers;

/// <summary>
/// Razor-thin Executive SuperAdmin Controller.
/// All user governance, role hierarchy validation (preventing self-demotion/self-blocking),
/// and metrics aggregation are strictly encapsulated in ISuperAdminService.
/// </summary>
[Authorize(Policy = "SuperAdminOnly")]
public class SuperAdminController : Controller
{
    private readonly ISuperAdminService _superAdminService;

    public SuperAdminController(ISuperAdminService superAdminService)
    {
        _superAdminService = superAdminService;
    }

    // =========================================================================
    // 1. DASHBOARD OVERVIEW
    // =========================================================================

    [HttpGet]
    public async Task<IActionResult> Dashboard(CancellationToken cancellationToken)
    {
        var dashboardDto = await _superAdminService.GetDashboardAsync(cancellationToken);
        return View(dashboardDto.ToViewModel());
    }

    // =========================================================================
    // 2. USERS GOVERNANCE TABLE
    // =========================================================================

    [HttpGet]
    public async Task<IActionResult> Users(
        string? searchTerm,
        int? searchId,
        UserRoleType? role,
        bool? isActive,
        int page = 1,
        CancellationToken cancellationToken = default)
    {
        var userListDto = await _superAdminService.GetUsersPagedAsync(
            searchTerm, searchId, role, isActive, page, 15, cancellationToken);

        return View(userListDto.ToViewModel());
    }

    // =========================================================================
    // 3. ACCOUNT AUDIT & ORDER DETAILS
    // =========================================================================

    [HttpGet]
    public async Task<IActionResult> UserDetails(int id, CancellationToken cancellationToken)
    {
        var user = await _superAdminService.GetUserDetailsAsync(id, cancellationToken);
        if (user == null)
        {
            TempData["ErrorMessage"] = $"User with ID #{id} was not found.";
            return RedirectToAction(nameof(Users));
        }

        return View(user.ToSuperAdminUserDetailsViewModel());
    }

    // =========================================================================
    // 4. PROMOTE / DEMOTE ROLE
    // =========================================================================

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PromoteRole(int userId, UserRoleType newRole, CancellationToken cancellationToken)
    {
        try
        {
            await _superAdminService.PromoteUserRoleAsync(User.GetUserId(), userId, newRole, cancellationToken);
            TempData["SuccessMessage"] = $"User #{userId} role was successfully updated to '{newRole}'.";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToAction(nameof(Users));
    }

    // =========================================================================
    // 5. TOGGLE ACCOUNT STATUS (ACTIVE / BLOCKED)
    // =========================================================================

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleUserStatus(int userId, CancellationToken cancellationToken)
    {
        try
        {
            var isNowActive = await _superAdminService.ToggleUserStatusAsync(User.GetUserId(), userId, cancellationToken);
            TempData["SuccessMessage"] = $"User #{userId} account is now {(isNowActive ? "Active" : "Blocked / Deactivated")}.";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToAction(nameof(Users));
    }

    // =========================================================================
    // 6. PROVISION ADMINISTRATOR ACCOUNT
    // =========================================================================

    [HttpGet]
    public IActionResult CreateAdmin()
    {
        return View(new CreateAdminAccountViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateAdmin(CreateAdminAccountViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var adminUser = await _superAdminService.CreateAdminAccountAsync(
                model.FullName,
                model.Email,
                model.Password,
                model.PhoneNumber,
                model.RoleType,
                cancellationToken);

            TempData["SuccessMessage"] = $"Administrator '{adminUser.FullName}' (ID: #{adminUser.Id}) was provisioned successfully.";
            return RedirectToAction(nameof(Users));
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(nameof(model.Email), ex.Message);
            return View(model);
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(model);
        }
    }
}
