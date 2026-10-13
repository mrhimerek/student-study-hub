using System.Security.Claims;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;
using StudentStudyHub.Application.Common.Interfaces;
using StudentStudyHub.Functions.Auth;

namespace StudentStudyHub.Functions.Middleware;

/// <summary>
/// Runs on every HTTP-triggered invocation. If a valid "Authorization: Bearer {jwt}" header is
/// present, validates the token and populates the scoped CurrentUserService so downstream
/// handlers can resolve ICurrentUserService without re-parsing the token themselves.
///
/// Anonymous endpoints (register/login/refresh) simply see an unset CurrentUserService.
/// Endpoints that require auth must check CurrentUserService.UserId themselves and return 401
/// when it is null - Azure Functions' isolated worker model has no built-in concept of
/// "[Authorize]" attributes, so this is the idiomatic way to centralize token validation while
/// leaving per-endpoint authorization decisions to the endpoint.
/// </summary>
public class JwtAuthMiddleware(IJwtTokenService jwtTokenService) : IFunctionsWorkerMiddleware
{
    public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
    {
        var httpContext = context.GetHttpContext();
        var authHeader = httpContext?.Request.Headers.Authorization.ToString();

        if (!string.IsNullOrWhiteSpace(authHeader) &&
            authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            var token = authHeader["Bearer ".Length..].Trim();
            var principal = jwtTokenService.ValidateAccessToken(token);

            var userIdClaim = principal?.FindFirst(ClaimTypes.NameIdentifier) ?? principal?.FindFirst("sub");
            var emailClaim = principal?.FindFirst(ClaimTypes.Email) ?? principal?.FindFirst("email");

            if (userIdClaim is not null && Guid.TryParse(userIdClaim.Value, out var userId))
            {
                if (context.InstanceServices.GetService(typeof(ICurrentUserService)) is CurrentUserService currentUser)
                {
                    currentUser.SetUser(userId, emailClaim?.Value ?? string.Empty);
                }
            }
        }

        await next(context);
    }
}
