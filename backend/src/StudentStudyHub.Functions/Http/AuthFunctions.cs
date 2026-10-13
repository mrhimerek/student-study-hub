using System.Net;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using StudentStudyHub.Application.Auth;
using StudentStudyHub.Domain.Exceptions;

namespace StudentStudyHub.Functions.Http;

/// <summary>
/// Anonymous auth endpoints: register, login, refresh, logout. Mirrors the AuthController
/// from the original App Service design, implemented as Azure Functions HTTP triggers.
/// </summary>
public class AuthFunctions(
    IAuthService authService,
    IValidator<RegisterRequest> registerValidator,
    IValidator<LoginRequest> loginValidator,
    IValidator<RefreshRequest> refreshValidator,
    ILogger<AuthFunctions> logger)
{
    [Function("Register")]
    public async Task<HttpResponseData> Register(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "auth/register")] HttpRequestData req)
    {
        var request = await req.ReadFromJsonAsync<RegisterRequest>();
        if (request is null)
        {
            return await BadRequestAsync(req, "Request body is required.");
        }

        var validation = await registerValidator.ValidateAsync(request);
        if (!validation.IsValid)
        {
            return await ValidationFailedAsync(req, validation);
        }

        try
        {
            var result = await authService.RegisterAsync(request);
            var response = req.CreateResponse(HttpStatusCode.Created);
            await response.WriteAsJsonAsync(result);
            return response;
        }
        catch (DomainException ex)
        {
            logger.LogInformation("Registration failed: {Message}", ex.Message);
            return await BadRequestAsync(req, ex.Message);
        }
    }

    [Function("Login")]
    public async Task<HttpResponseData> Login(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "auth/login")] HttpRequestData req)
    {
        var request = await req.ReadFromJsonAsync<LoginRequest>();
        if (request is null)
        {
            return await BadRequestAsync(req, "Request body is required.");
        }

        var validation = await loginValidator.ValidateAsync(request);
        if (!validation.IsValid)
        {
            return await ValidationFailedAsync(req, validation);
        }

        try
        {
            var result = await authService.LoginAsync(request);
            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(result);
            return response;
        }
        catch (DomainException ex)
        {
            return await UnauthorizedAsync(req, ex.Message);
        }
    }

    [Function("RefreshToken")]
    public async Task<HttpResponseData> Refresh(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "auth/refresh")] HttpRequestData req)
    {
        var request = await req.ReadFromJsonAsync<RefreshRequest>();
        if (request is null)
        {
            return await BadRequestAsync(req, "Request body is required.");
        }

        var validation = await refreshValidator.ValidateAsync(request);
        if (!validation.IsValid)
        {
            return await ValidationFailedAsync(req, validation);
        }

        try
        {
            var result = await authService.RefreshAsync(request.RefreshToken);
            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(result);
            return response;
        }
        catch (DomainException ex)
        {
            return await UnauthorizedAsync(req, ex.Message);
        }
        catch (NotFoundException ex)
        {
            return await UnauthorizedAsync(req, ex.Message);
        }
    }

    [Function("Logout")]
    public async Task<HttpResponseData> Logout(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "auth/logout")] HttpRequestData req)
    {
        var request = await req.ReadFromJsonAsync<RefreshRequest>();
        if (request is not null)
        {
            await authService.LogoutAsync(request.RefreshToken);
        }

        return req.CreateResponse(HttpStatusCode.NoContent);
    }

    private static async Task<HttpResponseData> BadRequestAsync(HttpRequestData req, string message)
    {
        var response = req.CreateResponse(HttpStatusCode.BadRequest);
        await response.WriteAsJsonAsync(new { error = message });
        return response;
    }

    private static async Task<HttpResponseData> UnauthorizedAsync(HttpRequestData req, string message)
    {
        var response = req.CreateResponse(HttpStatusCode.Unauthorized);
        await response.WriteAsJsonAsync(new { error = message });
        return response;
    }

    private static async Task<HttpResponseData> ValidationFailedAsync(HttpRequestData req, ValidationResult validation)
    {
        var response = req.CreateResponse(HttpStatusCode.BadRequest);
        await response.WriteAsJsonAsync(new { errors = validation.Errors.Select(e => e.ErrorMessage) });
        return response;
    }
}
