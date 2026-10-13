using System.Security.Claims;

namespace StudentStudyHub.Application.Common.Interfaces;

public interface ICurrentUserService
{
    Guid? UserId { get; }
    string? Email { get; }
}

public interface IDateTimeProvider
{
    DateTime UtcNow { get; }
}

public interface IJwtTokenService
{
    string GenerateAccessToken(Guid userId, string email);
    string GenerateRefreshToken();

    /// <summary>
    /// Validates a signed access token's signature, issuer/audience, and expiry.
    /// Returns the resulting principal, or null if the token is missing/invalid/expired.
    /// </summary>
    ClaimsPrincipal? ValidateAccessToken(string token);
}

public interface IPasswordHasher
{
    string Hash(string password);
    bool Verify(string password, string passwordHash);
}

public interface IEmailService
{
    Task SendEmailAsync(string toEmail, string subject, string htmlBody, CancellationToken cancellationToken = default);
}

public interface IBlobStorageService
{
    Task<string> UploadAsync(string containerName, string fileName, Stream content, string contentType, CancellationToken cancellationToken = default);
}
