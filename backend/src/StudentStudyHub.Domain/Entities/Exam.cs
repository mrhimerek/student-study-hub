using StudentStudyHub.Domain.Common;
using StudentStudyHub.Domain.Enums;

namespace StudentStudyHub.Domain.Entities;

public class Exam : BaseEntity
{
    public Guid UserId { get; set; }
    public User? User { get; set; }

    public string Subject { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public DateTime ExamDate { get; set; }
    public ExamType Type { get; set; } = ExamType.Exam;
    public string? Notes { get; set; }
}
