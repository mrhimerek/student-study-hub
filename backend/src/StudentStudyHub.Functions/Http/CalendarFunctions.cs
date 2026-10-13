using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using StudentStudyHub.Application.Calendar;
using StudentStudyHub.Application.Common.Interfaces;

namespace StudentStudyHub.Functions.Http;

/// <summary>
/// HTTP-triggered endpoint for the unified calendar (FR-11): aggregates Assignments, Exams,
/// Scholarships, College Applications, and the SAT target test date into one list of events for
/// a date range. Mirrors the "GET /calendar?from=&amp;to=" route from DESIGN.md Section "API Surface".
/// </summary>
public class CalendarFunctions(ICalendarService calendarService, ICurrentUserService currentUser, ILogger<CalendarFunctions> logger)
{
    [Function("GetCalendarEvents")]
    public async Task<HttpResponseData> Get(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "calendar")] HttpRequestData req)
    {
        if (currentUser.UserId is not { } userId)
        {
            return await ApiResponses.UnauthorizedAsync(req);
        }

        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        if (!TryParseDateParam(GetQueryParam(req, "from"), out var from))
        {
            from = new DateOnly(today.Year, today.Month, 1);
        }

        if (!TryParseDateParam(GetQueryParam(req, "to"), out var to))
        {
            to = from.AddMonths(1).AddDays(-1);
        }

        if (to < from)
        {
            return await ApiResponses.BadRequestAsync(req, "'to' must not be before 'from'.");
        }

        var events = await calendarService.GetEventsAsync(userId, from, to);

        logger.LogInformation("Calendar query for user {UserId}: {From}..{To} -> {Count} events", userId, from, to, events.Count);

        var response = req.CreateResponse(HttpStatusCode.OK);
        await response.WriteAsJsonAsync(events);
        return response;
    }

    private static bool TryParseDateParam(string? raw, out DateOnly value)
    {
        return DateOnly.TryParse(raw, out value);
    }

    private static string? GetQueryParam(HttpRequestData req, string name)
    {
        var query = req.Url.Query.TrimStart('?');
        foreach (var pair in query.Split('&', StringSplitOptions.RemoveEmptyEntries))
        {
            var parts = pair.Split('=', 2);
            if (parts.Length == 2 && Uri.UnescapeDataString(parts[0]).Equals(name, StringComparison.OrdinalIgnoreCase))
            {
                return Uri.UnescapeDataString(parts[1]);
            }
        }

        return null;
    }
}
