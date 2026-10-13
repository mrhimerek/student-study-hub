using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using StudentStudyHub.Application.Common.Interfaces;
using StudentStudyHub.Application.Notifications;
using StudentStudyHub.Domain.Exceptions;

namespace StudentStudyHub.Functions.Http;

/// <summary>
/// HTTP-triggered endpoints for the in-app notification inbox. Notification rows are created by
/// the DeadlineReminderFunction timer trigger; these endpoints are read/mark-as-read only.
/// </summary>
public class NotificationFunctions(INotificationService notificationService, ICurrentUserService currentUser, ILogger<NotificationFunctions> logger)
{
    [Function("ListNotifications")]
    public async Task<HttpResponseData> List(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "notifications")] HttpRequestData req)
    {
        if (currentUser.UserId is not { } userId)
        {
            return await ApiResponses.UnauthorizedAsync(req);
        }

        var unreadOnly = string.Equals(GetQueryParam(req, "unread"), "true", StringComparison.OrdinalIgnoreCase);
        var notifications = await notificationService.ListAsync(userId, unreadOnly);

        var response = req.CreateResponse(HttpStatusCode.OK);
        await response.WriteAsJsonAsync(notifications);
        return response;
    }

    [Function("GetUnreadNotificationCount")]
    public async Task<HttpResponseData> GetUnreadCount(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "notifications/unread-count")] HttpRequestData req)
    {
        if (currentUser.UserId is not { } userId)
        {
            return await ApiResponses.UnauthorizedAsync(req);
        }

        var count = await notificationService.GetUnreadCountAsync(userId);

        var response = req.CreateResponse(HttpStatusCode.OK);
        await response.WriteAsJsonAsync(new { count });
        return response;
    }

    [Function("MarkNotificationAsRead")]
    public async Task<HttpResponseData> MarkAsRead(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "notifications/{id:guid}/read")] HttpRequestData req,
        Guid id)
    {
        if (currentUser.UserId is not { } userId)
        {
            return await ApiResponses.UnauthorizedAsync(req);
        }

        try
        {
            await notificationService.MarkAsReadAsync(userId, id);
            return req.CreateResponse(HttpStatusCode.NoContent);
        }
        catch (NotFoundException ex)
        {
            return await ApiResponses.NotFoundAsync(req, ex.Message);
        }
    }

    [Function("MarkAllNotificationsAsRead")]
    public async Task<HttpResponseData> MarkAllAsRead(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "notifications/read-all")] HttpRequestData req)
    {
        if (currentUser.UserId is not { } userId)
        {
            return await ApiResponses.UnauthorizedAsync(req);
        }

        await notificationService.MarkAllAsReadAsync(userId);
        logger.LogInformation("User {UserId} marked all notifications as read", userId);
        return req.CreateResponse(HttpStatusCode.NoContent);
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
