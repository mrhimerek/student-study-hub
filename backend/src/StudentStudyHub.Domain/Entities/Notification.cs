using StudentStudyHub.Domain.Common;
using StudentStudyHub.Domain.Enums;

namespace StudentStudyHub.Domain.Entities;

public class Notification : BaseEntity
{
    public Guid UserId { get; set; }
    public User? User { get; set; }

    public NotificationType Type { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public Guid? RelatedEntityId { get; set; }
    public bool IsRead { get; set; }
    public bool SentViaEmail { get; set; }
}
