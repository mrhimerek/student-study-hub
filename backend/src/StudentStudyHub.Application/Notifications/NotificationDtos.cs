using StudentStudyHub.Domain.Enums;

namespace StudentStudyHub.Application.Notifications;

public record NotificationDto(
    Guid Id,
    NotificationType Type,
    string Title,
    string Body,
    Guid? RelatedEntityId,
    bool IsRead,
    bool SentViaEmail,
    DateTime CreatedAt);
