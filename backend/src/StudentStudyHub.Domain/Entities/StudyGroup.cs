using StudentStudyHub.Domain.Common;

namespace StudentStudyHub.Domain.Entities;

public class StudyGroup : BaseEntity
{
    public Guid SchoolId { get; set; }
    public School? School { get; set; }

    public Guid OwnerUserId { get; set; }
    public User? OwnerUser { get; set; }

    public string Name { get; set; } = string.Empty;
    public string? Subject { get; set; }

    /// <summary>8-character unique code students use to join this group.</summary>
    public string InviteCode { get; set; } = GenerateInviteCode();

    public ICollection<StudyGroupMember> Members { get; set; } = new List<StudyGroupMember>();
    public ICollection<GroupNote> Notes { get; set; } = new List<GroupNote>();

    private static string GenerateInviteCode()
    {
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789"; // no ambiguous chars
        var random = Random.Shared;
        return new string(Enumerable.Range(0, 8).Select(_ => chars[random.Next(chars.Length)]).ToArray());
    }
}
