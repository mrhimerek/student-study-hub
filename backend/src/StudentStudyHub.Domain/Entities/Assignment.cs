using StudentStudyHub.Domain.Common;
using StudentStudyHub.Domain.Enums;

namespace StudentStudyHub.Domain.Entities;

public class Assignment : BaseEntity
{
    public Guid UserId { get; set; }
    public User? User { get; set; }

    public string Subject { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime DueDate { get; set; }
    public Priority Priority { get; set; } = Priority.Medium;
    public AssignmentStatus Status { get; set; } = AssignmentStatus.NotStarted;
}
