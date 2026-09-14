using NexaMart.Application.Common.Models;
using NexaMart.Domain.Entities;
using NexaMart.Domain.Enums;

namespace NexaMart.Application.Interfaces.Services;

/// <summary>
/// Service contract for user management, profile updates, and role assignments.
/// </summary>
public interface IUserService
{
    Task<PagedResult<ApplicationUser>> GetAllUsersAsync(string? searchTerm = null, int? searchId = null, UserRoleType? role = null, bool? isActive = null, int pageIndex = 1, int pageSize = 10, CancellationToken cancellationToken = default);
    Task<ApplicationUser?> GetUserByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<ApplicationUser?> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<bool> PromoteUserRoleAsync(int id, UserRoleType newRole, CancellationToken cancellationToken = default);
    Task<bool> ToggleUserStatusAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> UpdateProfileAsync(int userId, string fullName, string? phoneNumber, string? currentPassword = null, string? newPassword = null, CancellationToken cancellationToken = default);
    Task<ApplicationUser> GetProfileAsync(int userId, CancellationToken cancellationToken = default);
    Task<(int TotalUsers, int Customers, int Admins, int SuperAdmins, int Blocked)> GetUserStatsAsync(CancellationToken cancellationToken = default);
    Task<ApplicationUser> CreateAdminAccountAsync(string fullName, string email, string password, string? phoneNumber, UserRoleType roleType, CancellationToken cancellationToken = default);
}
