using StudentStudyHub.Application.Common.Interfaces;
using StudentStudyHub.Domain.Entities;
using StudentStudyHub.Domain.Exceptions;

namespace StudentStudyHub.Application.Notifications;

/// <summary>
/// Read/mark-as-read operations over the in-app Notification inbox. Notification rows
/// themselves are created by DeadlineReminderService (the hourly scan), not here.
/// </summary>
public class NotificationService(IAppDbContext db, IDateTimeProvider clock) : INotificationService
{
    public Task<IReadOnlyList<NotificationDto>> ListAsync(Guid userId, bool unreadOnly = false, CancellationToken cancellationToken = default)
    {
        var query = db.Notifications.Where(n => n.UserId == userId);
        if (unreadOnly)
        {
            query = query.Where(n => !n.IsRead);
        }

        IReadOnlyList<NotificationDto> result = query
            .OrderByDescending(n => n.CreatedAt)
            .ToList()
            .Select(ToDto)
            .ToList();

        return Task.FromResult(result);
    }

    public Task<int> GetUnreadCountAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var count = db.Notifications.Count(n => n.UserId == userId && !n.IsRead);
        return Task.FromResult(count);
    }

    public async Task MarkAsReadAsync(Guid userId, Guid notificationId, CancellationToken cancellationToken = default)
    {
        var notification = db.Notifications.FirstOrDefault(n => n.Id == notificationId && n.UserId == userId)
            ?? throw new NotFoundException(nameof(Notification), notificationId);

        if (!notification.IsRead)
        {
            notification.IsRead = true;
            notification.UpdatedAt = clock.UtcNow;
            await db.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task MarkAllAsReadAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var unread = db.Notifications.Where(n => n.UserId == userId && !n.IsRead).ToList();
        foreach (var notification in unread)
        {
            notification.IsRead = true;
            notification.UpdatedAt = clock.UtcNow;
        }

        if (unread.Count > 0)
        {
            await db.SaveChangesAsync(cancellationToken);
        }
    }

    private static NotificationDto ToDto(Notification n) => new(
        n.Id, n.Type, n.Title, n.Body, n.RelatedEntityId, n.IsRead, n.SentViaEmail, n.CreatedAt);
}
