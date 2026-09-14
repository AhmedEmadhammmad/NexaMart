using Microsoft.EntityFrameworkCore;
using NexaMart.Application.Interfaces.Repositories;
using NexaMart.Application.Interfaces.Security;
using NexaMart.Application.Interfaces.Services;
using NexaMart.Domain.Entities;
using NexaMart.Domain.Enums;

namespace NexaMart.Application.Services;

/// <summary>
/// Custom authentication service implementing dual-mode login (Staff via ID, Customers via Email),
/// BCrypt password verification, and JWT session token generation without DTOs.
/// </summary>
public class AuthService : IAuthService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _jwtTokenService;

    public AuthService(
        IUnitOfWork unitOfWork,
        IPasswordHasher passwordHasher,
        IJwtTokenService jwtTokenService)
    {
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<(string Token, string RefreshToken, DateTime ExpiresAt, ApplicationUser User)> LoginAsync(
        string identifier,
        string password,
        bool rememberMe = false,
        CancellationToken cancellationToken = default)
    {
        var trimmedIdentifier = identifier.Trim();
        ApplicationUser? user = null;

        // 1. Check if identifier is numeric ID (Dedicated Staff Login for Admin, SuperAdmin)
        if (int.TryParse(trimmedIdentifier, out int numericId))
        {
            user = await _unitOfWork.Users.Query()
                .FirstOrDefaultAsync(u => u.Id == numericId, cancellationToken);

            if (user != null)
            {
                if (user.RoleType == UserRoleType.Customer)
                {
                    throw new UnauthorizedAccessException("Customer accounts must log in using their email address.");
                }
            }
        }

        // 2. If not found by ID or not numeric, lookup by Email
        if (user == null)
        {
            var email = trimmedIdentifier.ToLower();
            user = await _unitOfWork.Users.Query()
                .FirstOrDefaultAsync(u => u.Email.ToLower() == email, cancellationToken);
        }

        if (user == null)
        {
            throw new UnauthorizedAccessException("Invalid credentials.");
        }

        if (!user.IsActive)
        {
            throw new UnauthorizedAccessException("Your account has been deactivated. Please contact support.");
        }

        // 3. Cryptographic password verification
        bool isPasswordValid = _passwordHasher.VerifyPassword(password, user.PasswordHash);
        if (!isPasswordValid)
        {
            throw new UnauthorizedAccessException("Invalid credentials.");
        }

        // 4. Generate JWT access token & Refresh Token
        var token = _jwtTokenService.GenerateToken(user, out var expiresAt, rememberMe);
        var refreshToken = _jwtTokenService.GenerateRefreshToken();

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiresAt = rememberMe ? DateTime.UtcNow.AddDays(7) : DateTime.UtcNow.AddDays(1);
        user.LastLoginAt = DateTime.UtcNow;
        user.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Users.Update(user);
        await _unitOfWork.CompleteAsync(cancellationToken);

        return (token, refreshToken, expiresAt, user);
    }

    public async Task<(string Token, string RefreshToken, DateTime ExpiresAt, ApplicationUser User)> RegisterAsync(
        string fullName,
        string email,
        string password,
        string? phoneNumber = null,
        CancellationToken cancellationToken = default)
    {
        var normalizedEmail = email.Trim().ToLower();

        // Check for existing account
        var exists = await _unitOfWork.Users.Query()
            .AnyAsync(u => u.Email.ToLower() == normalizedEmail, cancellationToken);

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
            RoleType = UserRoleType.Customer,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = null
        };

        var refreshToken = _jwtTokenService.GenerateRefreshToken();
        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(7);

        await _unitOfWork.Users.AddAsync(user, cancellationToken);
        await _unitOfWork.CompleteAsync(cancellationToken);

        var token = _jwtTokenService.GenerateToken(user, out var expiresAt);

        return (token, refreshToken, expiresAt, user);
    }

    public async Task<(string Token, string RefreshToken, DateTime ExpiresAt, ApplicationUser User)> RefreshTokenAsync(
        string refreshToken,
        CancellationToken cancellationToken = default)
    {
        var user = await _unitOfWork.Users.Query()
            .FirstOrDefaultAsync(u => u.RefreshToken == refreshToken, cancellationToken);

        if (user == null || user.RefreshTokenExpiresAt < DateTime.UtcNow)
        {
            throw new UnauthorizedAccessException("Invalid or expired refresh token.");
        }

        var newToken = _jwtTokenService.GenerateToken(user, out var expiresAt);
        var newRefreshToken = _jwtTokenService.GenerateRefreshToken();

        user.RefreshToken = newRefreshToken;
        user.RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(7);
        user.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Users.Update(user);
        await _unitOfWork.CompleteAsync(cancellationToken);

        return (newToken, newRefreshToken, expiresAt, user);
    }

    public async Task<ApplicationUser?> GetProfileAsync(int userId, CancellationToken cancellationToken = default)
    {
        return await _unitOfWork.Users.GetByIdAsync(userId, cancellationToken);
    }

    public async Task ClearRefreshTokenAsync(int userId, CancellationToken cancellationToken = default)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(userId, cancellationToken);
        if (user != null)
        {
            user.RefreshToken = null;
            user.RefreshTokenExpiresAt = null;
            user.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.Users.Update(user);
            await _unitOfWork.CompleteAsync(cancellationToken);
        }
    }
}
