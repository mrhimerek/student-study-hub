using StudentStudyHub.Domain.Common;

namespace StudentStudyHub.Domain.Entities;

public class Course : BaseEntity
{
    public Guid UserId { get; set; }
    public User? User { get; set; }

    public string Name { get; set; } = string.Empty;
    public decimal CreditHours { get; set; }
    public string GradeValue { get; set; } = string.Empty; // "A", "B+", etc.
    public bool IsHonorsOrAp { get; set; }
    public string Term { get; set; } = string.Empty; // e.g. "Fall 2025"
}
