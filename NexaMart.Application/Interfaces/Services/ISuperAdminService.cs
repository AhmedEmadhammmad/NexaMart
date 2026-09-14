using NexaMart.Application.DTOs.SuperAdmin;
using NexaMart.Domain.Entities;
using NexaMart.Domain.Enums;

namespace NexaMart.Application.Interfaces.Services;

/// <summary>
/// Dedicated Application Service contract for Executive SuperAdmin operations.
/// Encapsulates user governance metrics, role promotion with authorization rules,
/// account status management, and administrator provisioning.
/// </summary>
public interface ISuperAdminService
{
    Task<SuperAdminDashboardDto> GetDashboardAsync(CancellationToken cancellationToken = default);
    Task<SuperAdminUserListDto> GetUsersPagedAsync(string? searchTerm, int? searchId, UserRoleType? role, bool? isActive, int page = 1, int pageSize = 15, CancellationToken cancellationToken = default);
    Task<ApplicationUser?> GetUserDetailsAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> PromoteUserRoleAsync(int currentUserId, int targetUserId, UserRoleType newRole, CancellationToken cancellationToken = default);
    Task<bool> ToggleUserStatusAsync(int currentUserId, int targetUserId, CancellationToken cancellationToken = default);
    Task<ApplicationUser> CreateAdminAccountAsync(string fullName, string email, string password, string? phoneNumber, UserRoleType roleType, CancellationToken cancellationToken = default);
}
