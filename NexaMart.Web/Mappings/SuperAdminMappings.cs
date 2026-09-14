using NexaMart.Application.Common.Models;
using NexaMart.Application.DTOs.SuperAdmin;
using NexaMart.Application.Interfaces.Security;
using NexaMart.Domain.Entities;
using NexaMart.Web.Models;

namespace NexaMart.Web.Mappings;

/// <summary>
/// SuperAdmin mapping extensions for User & Platform Management.
/// </summary>
public static partial class MappingExtensions
{
    public static SuperAdminUserListItemViewModel ToSuperAdminUserListItemViewModel(this ApplicationUser user)
    {
        return new SuperAdminUserListItemViewModel
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            RoleType = user.RoleType,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt,
            LastLoginAt = user.LastLoginAt,
            OrdersCount = user.Orders?.Count ?? 0,
            TotalSpent = user.Orders?.Sum(o => o.TotalAmount) ?? 0.00m
        };
    }

    public static SuperAdminUserDetailsViewModel ToSuperAdminUserDetailsViewModel(this ApplicationUser user)
    {
        return new SuperAdminUserDetailsViewModel
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            RoleType = user.RoleType,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt,
            LastLoginAt = user.LastLoginAt,
            UpdatedAt = user.UpdatedAt,
            Orders = user.Orders?.Select(o => o.ToDetailsViewModel()).ToList() ?? new List<OrderDetailsViewModel>()
        };
    }

    public static ApplicationUser ToEntity(this CreateAdminAccountViewModel model, IPasswordHasher passwordHasher)
    {
        return new ApplicationUser
        {
            FullName = model.FullName.Trim(),
            Email = model.Email.Trim().ToLowerInvariant(),
            PhoneNumber = model.PhoneNumber?.Trim(),
            PasswordHash = passwordHasher.HashPassword(model.Password),
            RoleType = model.RoleType,
            IsActive = true
        };
    }

    public static SuperAdminDashboardViewModel ToViewModel(this SuperAdminDashboardDto dto)
    {
        return new SuperAdminDashboardViewModel
        {
            TotalUsers = dto.TotalUsers,
            TotalCustomers = dto.TotalCustomers,
            TotalAdmins = dto.TotalAdmins,
            TotalSuperAdmins = dto.TotalSuperAdmins,
            BlockedUsersCount = dto.BlockedUsersCount,
            TotalProducts = dto.TotalProducts,
            TotalCategories = dto.TotalCategories,
            TotalOrders = dto.TotalOrders,
            TotalRevenue = dto.TotalRevenue,
            RecentRegisteredUsers = dto.RecentRegisteredUsers.Select(u => u.ToSuperAdminUserListItemViewModel()).ToList(),
            RecentOrders = dto.RecentOrders.Select(o => o.ToDetailsViewModel()).ToList()
        };
    }

    public static SuperAdminUserListViewModel ToViewModel(this SuperAdminUserListDto dto)
    {
        return new SuperAdminUserListViewModel
        {
            Users = new PagedResult<SuperAdminUserListItemViewModel>(
                dto.Users.Items.Select(u => u.ToSuperAdminUserListItemViewModel()).ToList(),
                dto.Users.TotalCount,
                dto.Users.PageIndex,
                dto.Users.PageSize),
            SearchTerm = dto.SearchTerm,
            RoleFilter = dto.RoleFilter,
            IsActiveFilter = dto.IsActiveFilter,
            AllUsersCount = dto.AllUsersCount,
            CustomersCount = dto.CustomersCount,
            AdminsCount = dto.AdminsCount,
            BlockedCount = dto.BlockedCount
        };
    }
}
