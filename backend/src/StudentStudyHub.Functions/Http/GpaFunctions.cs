using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using StudentStudyHub.Application.Common.Interfaces;
using StudentStudyHub.Application.Gpa;

namespace StudentStudyHub.Functions.Http;

/// <summary>
/// HTTP-triggered endpoints for the GPA feature. Mirrors the GpaController
/// from the App Service design, but as Azure Functions (isolated worker).
/// Pattern to replicate for Assignments/StudyGroups/Exams/etc. endpoints.
/// </summary>
public class GpaFunctions(IAppDbContext db, ICurrentUserService currentUser, ILogger<GpaFunctions> logger)
{
    [Function("GetGpaSummary")]
    public async Task<HttpResponseData> GetGpaSummary(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "gpa/summary")] HttpRequestData req)
    {
        // JwtAuthMiddleware has already validated the Bearer token (if any) and populated
        // ICurrentUserService; protected endpoints are responsible for enforcing auth themselves.
        if (currentUser.UserId is not { } userId)
        {
            var unauthorized = req.CreateResponse(HttpStatusCode.Unauthorized);
            await unauthorized.WriteAsJsonAsync(new { error = "Authentication required." });
            return unauthorized;
        }

        var courses = db.Courses
            .Where(c => c.UserId == userId)
            .Select(c => new GpaCalculator.CourseGrade(c.CreditHours, c.GradeValue, c.IsHonorsOrAp))
            .ToList();

        var summary = GpaCalculator.Calculate(courses);

        logger.LogInformation("Computed GPA summary for user {UserId}: {Summary}", userId, summary);

        var response = req.CreateResponse(HttpStatusCode.OK);
        await response.WriteAsJsonAsync(summary);
        return response;
    }
}
