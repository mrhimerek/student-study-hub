using StudentStudyHub.Domain.Common;
using StudentStudyHub.Domain.Enums;

namespace StudentStudyHub.Domain.Entities;

public class Scholarship : BaseEntity
{
    public Guid UserId { get; set; }
    public User? User { get; set; }

    public string Name { get; set; } = string.Empty;
    public decimal? Amount { get; set; }
    public DateOnly Deadline { get; set; }
    public ScholarshipStatus Status { get; set; } = ScholarshipStatus.Researching;

    /// <summary>Simple requirements checklist, stored as JSON (e.g. ["Essay", "2 Recommendation Letters"]).</summary>
    public string? RequirementsJson { get; set; }
}
