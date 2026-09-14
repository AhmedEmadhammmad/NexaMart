using NexaMart.Application.Interfaces.Security;

namespace NexaMart.Infrastructure.Security;

/// <summary>
/// High-security BCrypt password hasher with adaptive work factor (Cost 12) to resist brute-force attacks.
/// </summary>
public class BcryptPasswordHasher : IPasswordHasher
{
    private const int WorkFactor = 12;

    public string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password, workFactor: WorkFactor);
    }

    public bool VerifyPassword(string password, string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(passwordHash))
            return false;

        return BCrypt.Net.BCrypt.Verify(password, passwordHash);
    }
}
