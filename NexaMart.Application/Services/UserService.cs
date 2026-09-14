using Microsoft.EntityFrameworkCore;
using NexaMart.Application.Common.Models;
using NexaMart.Application.Interfaces.Repositories;
using NexaMart.Application.Interfaces.Security;
using NexaMart.Application.Interfaces.Services;
using NexaMart.Domain.Entities;
using NexaMart.Domain.Enums;

namespace NexaMart.Application.Services;

/// <summary>
/// Service implementing user management, profile updates, and role administration without DTOs.
/// </summary>
public class UserService : IUserService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;

    public UserService(IUnitOfWork unitOfWork, IPasswordHasher passwordHasher)
    {
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
    }

    public async Task<PagedResult<ApplicationUser>> GetAllUsersAsync(
        string? searchTerm = null,
        int? searchId = null,
        UserRoleType? role = null,
        bool? isActive = null,
        int pageIndex = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var query = _unitOfWork.Users.Query()
            .Include(u => u.Orders)
            .AsNoTracking();

        if (searchId.HasValue)
        {
            query = query.Where(u => u.Id == searchId.Value);
        }

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.Trim().ToLower();
            query = query.Where(u => u.FullName.ToLower().Contains(term)
                                  || u.Email.ToLower().Contains(term)
                                  || (u.PhoneNumber != null && u.PhoneNumber.Contains(term)));
        }

        if (role.HasValue)
        {
            query = query.Where(u => u.RoleType == role.Value);
        }

        if (isActive.HasValue)
        {
            query = query.Where(u => u.IsActive == isActive.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        pageIndex = pageIndex > 0 ? pageIndex : 1;
        pageSize = pageSize > 0 ? pageSize : 10;

        var users = await query
            .OrderByDescending(u => u.CreatedAt)
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<ApplicationUser>(users, totalCount, pageIndex, pageSize);
    }

    public async Task<ApplicationUser?> GetUserByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _unitOfWork.Users.Query()
            .Include(u => u.Orders)
                .ThenInclude(o => o.OrderItems)
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
    }

    public async Task<ApplicationUser?> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var normalized = email.Trim().ToLower();
        return await _unitOfWork.Users.Query()
            .Include(u => u.Orders)
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email.ToLower() == normalized, cancellationToken);
    }

    public async Task<bool> PromoteUserRoleAsync(int id, UserRoleType newRole, CancellationToken cancellationToken = default)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(id, cancellationToken);
        if (user == null)
        {
            throw new KeyNotFoundException($"User with ID {id} was not found.");
        }

        user.RoleType = newRole;
        user.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Users.Update(user);
        await _unitOfWork.CompleteAsync(cancellationToken);
        return true;
    }

    public async Task<bool> ToggleUserStatusAsync(int id, CancellationToken cancellationToken = default)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(id, cancellationToken);
        if (user == null)
        {
            throw new KeyNotFoundException($"User with ID {id} was not found.");
        }

        user.IsActive = !user.IsActive;
        user.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Users.Update(user);
        await _unitOfWork.CompleteAsync(cancellationToken);
        return user.IsActive;
    }

    public async Task<ApplicationUser> GetProfileAsync(int userId, CancellationToken cancellationToken = default)
    {
        var user = await GetUserByIdAsync(userId, cancellationToken);
        if (user == null)
        {
            throw new KeyNotFoundException($"User with ID {userId} was not found.");
        }
        return user;
    }

    public async Task<bool> UpdateProfileAsync(
        int userId,
        string fullName,
        string? phoneNumber,
        string? currentPassword = null,
        string? newPassword = null,
        CancellationToken cancellationToken = default)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(userId, cancellationToken);
        if (user == null)
        {
            throw new KeyNotFoundException($"User with ID {userId} was not found.");
        }

        user.FullName = fullName.Trim();
        user.PhoneNumber = phoneNumber?.Trim();

        if (!string.IsNullOrWhiteSpace(newPassword))
        {
            if (string.IsNullOrWhiteSpace(currentPassword))
            {
                throw new InvalidOperationException("Current password is required to set a new password.");
            }

            if (!_passwordHasher.VerifyPassword(currentPassword, user.PasswordHash))
            {
                throw new UnauthorizedAccessException("Current password does not match.");
            }

            user.PasswordHash = _passwordHasher.HashPassword(newPassword);
        }

        user.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Users.Update(user);
        await _unitOfWork.CompleteAsync(cancellationToken);
        return true;
    }

    public async Task<(int TotalUsers, int Customers, int Admins, int SuperAdmins, int Blocked)> GetUserStatsAsync(CancellationToken cancellationToken = default)
    {
        var stats = await _unitOfWork.Users.Query().AsNoTracking()
            .GroupBy(u => 1)
            .Select(g => new
            {
                Total = g.Count(),
                Customers = g.Sum(u => u.RoleType == UserRoleType.Customer ? 1 : 0),
                Admins = g.Sum(u => u.RoleType == UserRoleType.Admin ? 1 : 0),
                SuperAdmins = g.Sum(u => u.RoleType == UserRoleType.SuperAdmin ? 1 : 0),
                Blocked = g.Sum(u => !u.IsActive ? 1 : 0)
            })
            .FirstOrDefaultAsync(cancellationToken);

        return stats != null
            ? (stats.Total, stats.Customers, stats.Admins, stats.SuperAdmins, stats.Blocked)
            : (0, 0, 0, 0, 0);
    }

    public async Task<ApplicationUser> CreateAdminAccountAsync(
        string fullName,
        string email,
        string password,
        string? phoneNumber,
        UserRoleType roleType,
        CancellationToken cancellationToken = default)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();
        var exists = await _unitOfWork.Users.Query().AnyAsync(u => u.Email.ToLower() == normalizedEmail, cancellationToken);
        if (exists)
        {
            throw new InvalidOperationException("An account with this email address already exists.");
        }

        var user = new ApplicationUser
        {
            FullName = fullName.Trim(),
            Email = normalizedEmail,
            PasswordHash = _passwordHasher.HashPassword(password),
            PhoneNumber = phoneNumber?.Trim(),
            RoleType = roleType,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Users.AddAsync(user, cancellationToken);
        await _unitOfWork.CompleteAsync(cancellationToken);
        return user;
    }
}
