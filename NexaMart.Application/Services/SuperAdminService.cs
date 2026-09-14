using NexaMart.Application.DTOs.SuperAdmin;
using NexaMart.Application.Interfaces.Services;
using NexaMart.Domain.Entities;
using NexaMart.Domain.Enums;

namespace NexaMart.Application.Services;

/// <summary>
/// Dedicated Executive SuperAdmin Application Service.
/// Encapsulates all global user governance, role hierarchy validation, and staff provisioning.
/// </summary>
public class SuperAdminService : ISuperAdminService
{
    private readonly IUserService _userService;
    private readonly IProductService _productService;
    private readonly ICategoryService _categoryService;
    private readonly IOrderService _orderService;

    public SuperAdminService(
        IUserService userService,
        IProductService productService,
        ICategoryService categoryService,
        IOrderService orderService)
    {
        _userService = userService;
        _productService = productService;
        _categoryService = categoryService;
        _orderService = orderService;
    }

    public async Task<SuperAdminDashboardDto> GetDashboardAsync(CancellationToken cancellationToken = default)
    {
        var (totalUsers, customers, admins, superAdmins, blocked) = 
            await _userService.GetUserStatsAsync(cancellationToken);

        var (totalProducts, _, _, _, _) = await _productService.GetProductCatalogStatsAsync(cancellationToken);
        var categories = await _categoryService.GetAllCategoriesAsync(cancellationToken);
        var (totalOrders, totalRevenue) = await _orderService.GetOrderStatsAsync(cancellationToken);

        var recentUsersPaged = await _userService.GetAllUsersAsync(pageIndex: 1, pageSize: 8, cancellationToken: cancellationToken);
        var recentOrdersPaged = await _orderService.GetOrdersPagedAsync(pageIndex: 1, pageSize: 6, cancellationToken: cancellationToken);

        return new SuperAdminDashboardDto
        {
            TotalUsers = totalUsers,
            TotalCustomers = customers,
            TotalAdmins = admins,
            TotalSuperAdmins = superAdmins,
            BlockedUsersCount = blocked,
            TotalProducts = totalProducts,
            TotalCategories = categories.Count,
            TotalOrders = totalOrders,
            TotalRevenue = totalRevenue,
            RecentRegisteredUsers = recentUsersPaged.Items,
            RecentOrders = recentOrdersPaged.Items
        };
    }

    public async Task<SuperAdminUserListDto> GetUsersPagedAsync(
        string? searchTerm,
        int? searchId,
        UserRoleType? role,
        bool? isActive,
        int page = 1,
        int pageSize = 15,
        CancellationToken cancellationToken = default)
    {
        var pagedUsers = await _userService.GetAllUsersAsync(
            searchTerm: searchTerm,
            searchId: searchId,
            role: role,
            isActive: isActive,
            pageIndex: page,
            pageSize: pageSize,
            cancellationToken: cancellationToken);

        var (totalUsers, customers, admins, _, blocked) = 
            await _userService.GetUserStatsAsync(cancellationToken);

        return new SuperAdminUserListDto
        {
            Users = pagedUsers,
            SearchTerm = searchTerm,
            RoleFilter = role,
            IsActiveFilter = isActive,
            AllUsersCount = totalUsers,
            CustomersCount = customers,
            AdminsCount = admins,
            BlockedCount = blocked
        };
    }

    public async Task<ApplicationUser?> GetUserDetailsAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _userService.GetUserByIdAsync(id, cancellationToken);
    }

    public async Task<bool> PromoteUserRoleAsync(
        int currentUserId,
        int targetUserId,
        UserRoleType newRole,
        CancellationToken cancellationToken = default)
    {
        if (currentUserId == targetUserId)
        {
            throw new InvalidOperationException("Security Restriction: You cannot modify your own administrative role.");
        }

        return await _userService.PromoteUserRoleAsync(targetUserId, newRole, cancellationToken);
    }

    public async Task<bool> ToggleUserStatusAsync(
        int currentUserId,
        int targetUserId,
        CancellationToken cancellationToken = default)
    {
        if (currentUserId == targetUserId)
        {
            throw new InvalidOperationException("Security Restriction: You cannot deactivate or block your own account.");
        }

        return await _userService.ToggleUserStatusAsync(targetUserId, cancellationToken);
    }

    public async Task<ApplicationUser> CreateAdminAccountAsync(
        string fullName,
        string email,
        string password,
        string? phoneNumber,
        UserRoleType roleType,
        CancellationToken cancellationToken = default)
    {
        return await _userService.CreateAdminAccountAsync(
            fullName,
            email,
            password,
            phoneNumber,
            roleType,
            cancellationToken);
    }
}
