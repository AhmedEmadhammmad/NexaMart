namespace NexaMart.Application.Interfaces.Security;

/// <summary>
/// Password hashing contract supporting secure salting and cryptographic verification.
/// </summary>
public interface IPasswordHasher
{
    string HashPassword(string password);
    bool VerifyPassword(string password, string passwordHash);
}
