using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NexaMart.Application.Interfaces.Services;
using NexaMart.Domain.Entities;
using NexaMart.Domain.Enums;
using NexaMart.Web.Extensions;
using NexaMart.Web.Mappings;
using NexaMart.Web.Models;

namespace NexaMart.Web.Controllers;

/// <summary>
/// Controller handling user authentication, staff portal login (Admin/SuperAdmin by ID & Role),
/// customer registration, and user profile management.
/// </summary>
public class AccountController : Controller
{
    private readonly IAuthService _authService;
    private readonly IUserService _userService;
    private readonly ILogger<AccountController> _logger;

    public AccountController(
        IAuthService authService,
        IUserService userService,
        ILogger<AccountController> logger)
    {
        _authService = authService;
        _userService = userService;
        _logger = logger;
    }

    // =========================================================================
    // 1. CUSTOMER LOGIN (Email + Password)
    // =========================================================================

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToRoleDashboard();
        }

        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var (_, _, _, user) = await _authService.LoginAsync(
                model.Identifier,
                model.Password,
                model.RememberMe,
                HttpContext.RequestAborted);

            await SignInUserAsync(user, model.RememberMe);

            _logger.LogInformation("User '{Email}' (ID: {Id}, Role: {Role}) logged in successfully.",
                user.Email, user.Id, user.RoleType);

            // Redirect based on role or returnUrl
            if (user.RoleType == UserRoleType.SuperAdmin)
            {
                return RedirectToAction("Dashboard", "SuperAdmin");
            }

            if (user.RoleType == UserRoleType.Admin)
            {
                return RedirectToAction("Dashboard", "Admin");
            }

            if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
            {
                return Redirect(model.ReturnUrl);
            }

            return RedirectToAction("Index", "Home");
        }
        catch (UnauthorizedAccessException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during login for {Identifier}", model.Identifier);
            ModelState.AddModelError(string.Empty, "An unexpected error occurred. Please try again.");
            return View(model);
        }
    }

    // =========================================================================
    // 2. LEGACY STAFF LOGIN REDIRECT (Redirects to Unified Login)
    // =========================================================================

    [HttpGet]
    public IActionResult StaffLogin(string? returnUrl = null)
    {
        return RedirectToAction(nameof(Login), new { returnUrl });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult StaffLogin(StaffLoginViewModel model)
    {
        return RedirectToAction(nameof(Login), new { returnUrl = model.ReturnUrl });
    }

    // =========================================================================
    // 3. REGISTRATION (Customer Only)
    // =========================================================================

    [HttpGet]
    public IActionResult Register(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Home");
        }

        ViewData["ReturnUrl"] = returnUrl;
        return View(new RegisterViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model, string? returnUrl = null)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var (_, _, _, user) = await _authService.RegisterAsync(
                model.FullName,
                model.Email,
                model.Password,
                model.PhoneNumber,
                HttpContext.RequestAborted);

            await SignInUserAsync(user, rememberMe: false);

            _logger.LogInformation("New customer account registered: {Email} (ID: {Id})", user.Email, user.Id);

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Index", "Home");
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(nameof(model.Email), ex.Message);
            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during registration for {Email}", model.Email);
            ModelState.AddModelError(string.Empty, "Registration failed. Please try again.");
            return View(model);
        }
    }

    // =========================================================================
    // 4. LOGOUT
    // =========================================================================

    [HttpPost]
    [HttpGet]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        _logger.LogInformation("User logged out.");
        return RedirectToAction("Login", "Account");
    }

    // =========================================================================
    // 5. USER PROFILE
    // =========================================================================

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> Profile(CancellationToken cancellationToken)
    {
        var user = await _userService.GetProfileAsync(User.GetUserId(), cancellationToken);
        return View(user.ToProfileViewModel());
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Profile(ProfileViewModel model, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();

        if (!ModelState.IsValid)
        {
            var currentUser = await _userService.GetProfileAsync(userId, cancellationToken);
            model.Email = currentUser.Email;
            model.RoleType = currentUser.RoleType;
            model.CreatedAt = currentUser.CreatedAt;
            return View(model);
        }

        try
        {
            await _userService.UpdateProfileAsync(
                userId,
                model.FullName,
                model.PhoneNumber,
                model.CurrentPassword,
                model.NewPassword,
                cancellationToken);

            TempData["SuccessMessage"] = "Your profile has been updated successfully.";
            return RedirectToAction(nameof(Profile));
        }
        catch (UnauthorizedAccessException ex)
        {
            ModelState.AddModelError(nameof(model.CurrentPassword), ex.Message);
            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating profile for user {UserId}", userId);
            ModelState.AddModelError(string.Empty, "Failed to update profile. Please check your inputs.");
            return View(model);
        }
    }

    // =========================================================================
    // 6. ACCESS DENIED
    // =========================================================================

    [HttpGet]
    public IActionResult AccessDenied()
    {
        return View();
    }

    // =========================================================================
    // HELPER METHODS
    // =========================================================================

    private async Task SignInUserAsync(ApplicationUser user, bool rememberMe)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.FullName),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, user.RoleType.ToString()),
            new("PhoneNumber", user.PhoneNumber ?? string.Empty)
        };

        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

        var authProperties = new AuthenticationProperties
        {
            IsPersistent = rememberMe,
            ExpiresUtc = rememberMe
                ? DateTimeOffset.UtcNow.AddDays(2)
                : DateTimeOffset.UtcNow.AddHours(2),
            AllowRefresh = true
        };

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(claimsIdentity),
            authProperties);
    }

    private IActionResult RedirectToRoleDashboard()
    {
        if (User.IsInRole("SuperAdmin"))
        {
            return RedirectToAction("Dashboard", "SuperAdmin");
        }

        if (User.IsInRole("Admin"))
        {
            return RedirectToAction("Dashboard", "Admin");
        }

        return RedirectToAction("Index", "Home");
    }
}
