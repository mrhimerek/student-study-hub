using StudentStudyHub.Domain.Enums;

namespace StudentStudyHub.Application.StudyGroups;

public record StudyGroupDto(
    Guid Id,
    string Name,
    string? Subject,
    string InviteCode,
    string SchoolName,
    Guid OwnerUserId,
    bool IsOwner,
    int MemberCount,
    DateTime CreatedAt);

public record StudyGroupMemberDto(
    Guid UserId,
    string FirstName,
    string LastName,
    GroupMemberRole Role,
    DateTime JoinedAt);

public record StudyGroupDetailDto(
    Guid Id,
    string Name,
    string? Subject,
    string InviteCode,
    string SchoolName,
    Guid OwnerUserId,
    bool IsOwner,
    IReadOnlyList<StudyGroupMemberDto> Members,
    DateTime CreatedAt);

/// <summary>
/// SchoolName is matched against existing schools by exact trimmed name, and a new School row is
/// created on first use - there is no separate School CRUD/picker UI yet (see REMAINING_WORK.md).
/// </summary>
public record CreateStudyGroupRequest(string Name, string? Subject, string SchoolName);

public record UpdateStudyGroupRequest(string Name, string? Subject);

public record JoinStudyGroupRequest(string InviteCode);
