using System.Security.Cryptography;
using System.Text;
using StudentStudyHub.Application.Common.Interfaces;
using StudentStudyHub.Application.Common.Settings;
using StudentStudyHub.Domain.Entities;
using StudentStudyHub.Domain.Exceptions;

namespace StudentStudyHub.Application.Auth;

/// <summary>
/// Orchestrates registration, login, and refresh-token rotation. Access tokens are short-lived
/// signed JWTs (never persisted); refresh tokens are long, random, opaque strings - only a
/// SHA-256 hash of the refresh token is ever stored, so a leaked database never exposes usable
/// tokens. Each refresh rotates the token (old one revoked, new one issued) to limit replay risk.
/// </summary>
public class AuthService(
    IAppDbContext db,
    IPasswordHasher passwordHasher,
    IJwtTokenService jwtTokenService,
    IDateTimeProvider clock,
    JwtSettings jwtSettings) : IAuthService
{
    public async Task<AuthResult> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();

        if (db.Users.Any(u => u.Email == normalizedEmail))
        {
            throw new DomainException("An account with this email already exists.");
        }

        var user = new User
        {
            Email = normalizedEmail,
            PasswordHash = passwordHasher.Hash(request.Password),
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            GradeLevel = request.GradeLevel,
            SchoolId = request.SchoolId
        };

        db.Add(user);
        await db.SaveChangesAsync(cancellationToken);

        return await IssueTokensAsync(user, cancellationToken);
    }

    public async Task<AuthResult> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();

        var user = db.Users.FirstOrDefault(u => u.Email == normalizedEmail);
        if (user is null || !passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            throw new DomainException("Invalid email or password.");
        }

        return await IssueTokensAsync(user, cancellationToken);
    }

    public async Task<AuthResult> RefreshAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        var tokenHash = HashToken(refreshToken);
        var existing = db.RefreshTokens.FirstOrDefault(rt => rt.TokenHash == tokenHash);

        if (existing is null || !existing.IsActive)
        {
            throw new DomainException("Refresh token is invalid, expired, or has been revoked.");
        }

        var user = db.Users.FirstOrDefault(u => u.Id == existing.UserId)
            ?? throw new NotFoundException(nameof(User), existing.UserId);

        existing.RevokedAt = clock.UtcNow;

        return await IssueTokensAsync(user, cancellationToken);
    }

    public async Task LogoutAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        var tokenHash = HashToken(refreshToken);
        var existing = db.RefreshTokens.FirstOrDefault(rt => rt.TokenHash == tokenHash);

        if (existing is not null && existing.IsActive)
        {
            existing.RevokedAt = clock.UtcNow;
            await db.SaveChangesAsync(cancellationToken);
        }
    }

    private async Task<AuthResult> IssueTokensAsync(User user, CancellationToken cancellationToken)
    {
        var accessToken = jwtTokenService.GenerateAccessToken(user.Id, user.Email);
        var refreshTokenPlain = jwtTokenService.GenerateRefreshToken();

        db.Add(new RefreshToken
        {
            UserId = user.Id,
            TokenHash = HashToken(refreshTokenPlain),
            ExpiresAt = clock.UtcNow.AddDays(jwtSettings.RefreshTokenDays)
        });

        await db.SaveChangesAsync(cancellationToken);

        return new AuthResult(
            accessToken,
            refreshTokenPlain,
            clock.UtcNow.AddMinutes(jwtSettings.AccessTokenMinutes),
            new UserDto(user.Id, user.Email, user.FirstName, user.LastName, user.GradeLevel, user.SchoolId));
    }

    private static string HashToken(string value)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(value));
        return Convert.ToHexString(bytes);
    }
}
