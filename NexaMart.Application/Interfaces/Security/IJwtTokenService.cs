using NexaMart.Domain.Entities;

namespace NexaMart.Application.Interfaces.Security;

/// <summary>
/// Contract for issuing signed JSON Web Tokens and refresh tokens.
/// </summary>
public interface IJwtTokenService
{
    string GenerateToken(ApplicationUser user, out DateTime expiresAt, bool rememberMe = false);
    string GenerateRefreshToken();
}
