using StudentStudyHub.Application.Common.Interfaces;

namespace StudentStudyHub.Infrastructure.Identity;

/// <summary>
/// Wraps BCrypt.Net-Next (an "enhanced" variant of bcrypt that pre-hashes the password with
/// SHA-384 to remove bcrypt's 72-byte input truncation) for password storage.
/// </summary>
public class PasswordHasher : IPasswordHasher
{
    private const int WorkFactor = 12;

    public string Hash(string password) =>
        BCrypt.Net.BCrypt.EnhancedHashPassword(password, workFactor: WorkFactor);

    public bool Verify(string password, string passwordHash)
    {
        try
        {
            return BCrypt.Net.BCrypt.EnhancedVerify(password, passwordHash);
        }
        catch (BCrypt.Net.SaltParseException)
        {
            return false;
        }
    }
}
