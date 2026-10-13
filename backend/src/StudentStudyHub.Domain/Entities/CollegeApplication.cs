using StudentStudyHub.Domain.Common;
using StudentStudyHub.Domain.Enums;

namespace StudentStudyHub.Domain.Entities;

public class CollegeApplication : BaseEntity
{
    public Guid UserId { get; set; }
    public User? User { get; set; }

    public string CollegeName { get; set; } = string.Empty;
    public ApplicationType ApplicationType { get; set; } = ApplicationType.RegularDecision;
    public DateOnly Deadline { get; set; }
    public CollegeApplicationStatus Status { get; set; } = CollegeApplicationStatus.NotStarted;

    /// <summary>Simple requirements checklist, stored as JSON.</summary>
    public string? RequirementsJson { get; set; }
}
