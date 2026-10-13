using System.Net;
using FluentValidation.Results;
using Microsoft.Azure.Functions.Worker.Http;

namespace StudentStudyHub.Functions.Http;

/// <summary>
/// Shared helpers for building consistent JSON error responses across HTTP-triggered
/// functions. New feature endpoints (Assignments, Exams, StudyGroups, ...) should use
/// these rather than hand-rolling response bodies.
/// </summary>
internal static class ApiResponses
{
    public static async Task<HttpResponseData> BadRequestAsync(HttpRequestData req, string message)
    {
        var response = req.CreateResponse(HttpStatusCode.BadRequest);
        await response.WriteAsJsonAsync(new { error = message });
        return response;
    }

    public static async Task<HttpResponseData> UnauthorizedAsync(HttpRequestData req, string message = "Authentication required.")
    {
        var response = req.CreateResponse(HttpStatusCode.Unauthorized);
        await response.WriteAsJsonAsync(new { error = message });
        return response;
    }

    public static async Task<HttpResponseData> NotFoundAsync(HttpRequestData req, string message)
    {
        var response = req.CreateResponse(HttpStatusCode.NotFound);
        await response.WriteAsJsonAsync(new { error = message });
        return response;
    }

    public static async Task<HttpResponseData> ForbiddenAsync(HttpRequestData req, string message)
    {
        var response = req.CreateResponse(HttpStatusCode.Forbidden);
        await response.WriteAsJsonAsync(new { error = message });
        return response;
    }

    public static async Task<HttpResponseData> ValidationFailedAsync(HttpRequestData req, ValidationResult validation)
    {
        var response = req.CreateResponse(HttpStatusCode.BadRequest);
        await response.WriteAsJsonAsync(new { errors = validation.Errors.Select(e => e.ErrorMessage) });
        return response;
    }
}
