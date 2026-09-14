using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using NexaMart.Application.Interfaces.Security;
using NexaMart.Domain.Entities;

namespace NexaMart.Infrastructure.Security;

/// <summary>
/// Production-grade JWT token generator embedding identity claims and symmetric signature.
/// </summary>
public class JwtTokenService : IJwtTokenService
{
    private readonly IConfiguration _configuration;

    public JwtTokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GenerateToken(ApplicationUser user, out DateTime expiresAt, bool rememberMe = false)
    {
        var secretKey = _configuration["JwtSettings:SecretKey"] 
            ?? throw new InvalidOperationException("JWT SecretKey is not configured in appsettings.json.");
        var issuer = _configuration["JwtSettings:Issuer"] ?? "NexaMart";
        var audience = _configuration["JwtSettings:Audience"] ?? "NexaMart.Clients";
        var expiryMinutesStr = _configuration["JwtSettings:ExpiryMinutes"];
        var expiryMinutes = double.TryParse(expiryMinutesStr, out var exp) ? exp : 120;

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Name, user.FullName),
            new(ClaimTypes.Role, user.RoleType.ToString()),
            new("RoleType", user.RoleType.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        // If RememberMe is selected, keep session and token valid for configured days (defaults to 2 days)
        var rememberMeDaysStr = _configuration["JwtSettings:RememberMeDays"];
        var rememberMeDays = double.TryParse(rememberMeDaysStr, out var rDays) ? rDays : 2;

        expiresAt = rememberMe ? DateTime.UtcNow.AddDays(rememberMeDays) : DateTime.UtcNow.AddMinutes(expiryMinutes);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = expiresAt,
            Issuer = issuer,
            Audience = audience,
            SigningCredentials = credentials
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);

        return tokenHandler.WriteToken(token);
    }

    public string GenerateRefreshToken()
    {
        var randomNumber = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }
}
