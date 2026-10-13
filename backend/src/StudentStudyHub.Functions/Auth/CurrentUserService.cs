using StudentStudyHub.Application.Common.Interfaces;

namespace StudentStudyHub.Functions.Auth;

/// <summary>
/// Scoped (per-invocation) implementation of ICurrentUserService. Populated by
/// JwtAuthMiddleware after it validates the request's Bearer token.
/// </summary>
public class CurrentUserService : ICurrentUserService
{
    public Guid? UserId { get; private set; }
    public string? Email { get; private set; }

    public void SetUser(Guid userId, string email)
    {
        UserId = userId;
        Email = email;
    }
}
