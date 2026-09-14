using NexaMart.Domain.Entities;
using NexaMart.Domain.Enums;

namespace NexaMart.Application.Interfaces.Services;

/// <summary>
/// Authentication service contract handling login, registration, and session renewal.
/// </summary>
public interface IAuthService
{
    Task<(string Token, string RefreshToken, DateTime ExpiresAt, ApplicationUser User)> LoginAsync(string identifier, string password, bool rememberMe = false, CancellationToken cancellationToken = default);
    Task<(string Token, string RefreshToken, DateTime ExpiresAt, ApplicationUser User)> RegisterAsync(string fullName, string email, string password, string? phoneNumber = null, CancellationToken cancellationToken = default);
    Task<(string Token, string RefreshToken, DateTime ExpiresAt, ApplicationUser User)> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);
    Task<ApplicationUser?> GetProfileAsync(int userId, CancellationToken cancellationToken = default);
    Task ClearRefreshTokenAsync(int userId, CancellationToken cancellationToken = default);
}
