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
public class GpaFunctions(IAppDbContext db, ILogger<GpaFunctions> logger)
{
    [Function("GetGpaSummary")]
    public async Task<HttpResponseData> GetGpaSummary(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "gpa/summary")] HttpRequestData req,
        FunctionContext context)
    {
        // TODO: resolve the authenticated user id from the validated JWT (ICurrentUserService),
        // once the JWT validation middleware (Middleware/JwtAuthMiddleware) is wired up.
        var userId = GetUserIdFromQuery(req);

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

    private static Guid GetUserIdFromQuery(HttpRequestData req)
    {
        var query = System.Web.HttpUtility.ParseQueryString(req.Url.Query);
        return Guid.TryParse(query["userId"], out var id) ? id : Guid.Empty;
    }
}
