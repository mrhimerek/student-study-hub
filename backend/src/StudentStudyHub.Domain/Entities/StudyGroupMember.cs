using StudentStudyHub.Domain.Enums;

namespace StudentStudyHub.Domain.Entities;

/// <summary>
/// Join entity between User and StudyGroup. Uses a composite key
/// (StudyGroupId, UserId) configured in EF Core Fluent API, so it does not
/// derive from BaseEntity.
/// </summary>
public class StudyGroupMember
{
    public Guid StudyGroupId { get; set; }
    public StudyGroup? StudyGroup { get; set; }

    public Guid UserId { get; set; }
    public User? User { get; set; }

    public GroupMemberRole Role { get; set; } = GroupMemberRole.Member;
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
}
