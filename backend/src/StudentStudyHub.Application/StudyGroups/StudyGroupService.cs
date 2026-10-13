using StudentStudyHub.Application.Common.Interfaces;
using StudentStudyHub.Domain.Entities;
using StudentStudyHub.Domain.Enums;
using StudentStudyHub.Domain.Exceptions;

namespace StudentStudyHub.Application.StudyGroups;

/// <summary>
/// CRUD + membership management for study groups. Unlike Assignments/Courses/Exams (which are
/// scoped by a single owning UserId column), a study group is shared by multiple users, so every
/// operation is instead scoped by *membership*: GetAsync/UpdateAsync/DeleteAsync/leaving all throw
/// NotFoundException for a group the caller doesn't belong to (so its existence isn't leaked),
/// while owner-only actions (update/delete) throw ForbiddenException for a member who isn't the
/// owner (since that member already knows the group exists).
/// </summary>
public class StudyGroupService(IAppDbContext db, IDateTimeProvider clock) : IStudyGroupService
{
    public Task<IReadOnlyList<StudyGroupDto>> ListMineAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var myMemberships = db.StudyGroupMembers.Where(m => m.UserId == userId).ToList();
        var groupIds = myMemberships.Select(m => m.StudyGroupId).ToList();

        var groups = db.StudyGroups.Where(g => groupIds.Contains(g.Id)).ToList();
        var schoolNamesById = SchoolNamesById(groups.Select(g => g.SchoolId));
        var memberCountsByGroup = MemberCountsByGroup(groupIds);

        IReadOnlyList<StudyGroupDto> result = groups
            .OrderBy(g => g.Name)
            .Select(g => ToDto(g, userId, schoolNamesById, memberCountsByGroup))
            .ToList();
        return Task.FromResult(result);
    }

    public Task<StudyGroupDetailDto> GetAsync(Guid userId, Guid groupId, CancellationToken cancellationToken = default)
    {
        var group = FindGroupForMember(userId, groupId);

        var memberRows = db.StudyGroupMembers.Where(m => m.StudyGroupId == groupId).ToList();
        var usersById = db.Users.Where(u => memberRows.Select(m => m.UserId).Contains(u.Id)).ToList()
            .ToDictionary(u => u.Id);
        var schoolName = SchoolNamesById([group.SchoolId]).GetValueOrDefault(group.SchoolId, "Unknown school");

        var members = memberRows
            .OrderByDescending(m => m.Role) // Owner (1) before Member (0)
            .ThenBy(m => m.JoinedAt)
            .Select(m =>
            {
                var user = usersById.GetValueOrDefault(m.UserId);
                return new StudyGroupMemberDto(m.UserId, user?.FirstName ?? "Unknown", user?.LastName ?? "", m.Role, m.JoinedAt);
            })
            .ToList();

        var detail = new StudyGroupDetailDto(
            group.Id, group.Name, group.Subject, group.InviteCode, schoolName,
            group.OwnerUserId, group.OwnerUserId == userId, members, group.CreatedAt);
        return Task.FromResult(detail);
    }

    public async Task<StudyGroupDto> CreateAsync(Guid userId, CreateStudyGroupRequest request, CancellationToken cancellationToken = default)
    {
        var schoolName = request.SchoolName.Trim();
        var school = db.Schools.FirstOrDefault(s => s.Name == schoolName);
        if (school is null)
        {
            school = new School { Name = schoolName };
            db.Add(school);
        }

        var group = new StudyGroup
        {
            SchoolId = school.Id,
            OwnerUserId = userId,
            Name = request.Name.Trim(),
            Subject = string.IsNullOrWhiteSpace(request.Subject) ? null : request.Subject.Trim(),
        };
        db.Add(group);

        db.Add(new StudyGroupMember
        {
            StudyGroupId = group.Id,
            UserId = userId,
            Role = GroupMemberRole.Owner,
            JoinedAt = clock.UtcNow,
        });

        await db.SaveChangesAsync(cancellationToken);

        return new StudyGroupDto(group.Id, group.Name, group.Subject, group.InviteCode, school.Name, userId, true, 1, group.CreatedAt);
    }

    public async Task<StudyGroupDto> UpdateAsync(Guid userId, Guid groupId, UpdateStudyGroupRequest request, CancellationToken cancellationToken = default)
    {
        var group = FindGroupForMember(userId, groupId);
        if (group.OwnerUserId != userId)
        {
            throw new ForbiddenException("Only the group owner can edit this study group.");
        }

        group.Name = request.Name.Trim();
        group.Subject = string.IsNullOrWhiteSpace(request.Subject) ? null : request.Subject.Trim();
        group.UpdatedAt = clock.UtcNow;

        await db.SaveChangesAsync(cancellationToken);

        var schoolName = SchoolNamesById([group.SchoolId]).GetValueOrDefault(group.SchoolId, "Unknown school");
        var memberCount = db.StudyGroupMembers.Count(m => m.StudyGroupId == groupId);
        return ToDto(group, userId, new Dictionary<Guid, string> { [group.SchoolId] = schoolName }, new Dictionary<Guid, int> { [group.Id] = memberCount });
    }

    public async Task<StudyGroupDto> JoinAsync(Guid userId, JoinStudyGroupRequest request, CancellationToken cancellationToken = default)
    {
        var code = request.InviteCode.Trim().ToUpperInvariant();
        var group = db.StudyGroups.FirstOrDefault(g => g.InviteCode == code)
            ?? throw new DomainException($"No study group found with invite code '{request.InviteCode}'.");

        var alreadyMember = db.StudyGroupMembers.Any(m => m.StudyGroupId == group.Id && m.UserId == userId);
        if (alreadyMember)
        {
            throw new DomainException("You're already a member of this study group.");
        }

        db.Add(new StudyGroupMember
        {
            StudyGroupId = group.Id,
            UserId = userId,
            Role = GroupMemberRole.Member,
            JoinedAt = clock.UtcNow,
        });
        await db.SaveChangesAsync(cancellationToken);

        var schoolName = SchoolNamesById([group.SchoolId]).GetValueOrDefault(group.SchoolId, "Unknown school");
        var memberCount = db.StudyGroupMembers.Count(m => m.StudyGroupId == group.Id);
        return ToDto(group, userId, new Dictionary<Guid, string> { [group.SchoolId] = schoolName }, new Dictionary<Guid, int> { [group.Id] = memberCount });
    }

    public async Task LeaveAsync(Guid userId, Guid groupId, CancellationToken cancellationToken = default)
    {
        var membership = db.StudyGroupMembers.FirstOrDefault(m => m.StudyGroupId == groupId && m.UserId == userId)
            ?? throw new NotFoundException(nameof(StudyGroup), groupId);

        if (membership.Role == GroupMemberRole.Owner)
        {
            throw new DomainException("Group owners can't leave their own group. Delete the group instead.");
        }

        db.Remove(membership);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid userId, Guid groupId, CancellationToken cancellationToken = default)
    {
        var group = FindGroupForMember(userId, groupId);
        if (group.OwnerUserId != userId)
        {
            throw new ForbiddenException("Only the group owner can delete this study group.");
        }

        // The DB cascades these on delete, but FakeAppDbContext (used by unit tests) doesn't
        // simulate cascading deletes, so remove child rows explicitly for correctness everywhere.
        foreach (var member in db.StudyGroupMembers.Where(m => m.StudyGroupId == groupId).ToList())
        {
            db.Remove(member);
        }

        foreach (var note in db.GroupNotes.Where(n => n.StudyGroupId == groupId).ToList())
        {
            db.Remove(note);
        }

        db.Remove(group);
        await db.SaveChangesAsync(cancellationToken);
    }

    private StudyGroup FindGroupForMember(Guid userId, Guid groupId)
    {
        var isMember = db.StudyGroupMembers.Any(m => m.StudyGroupId == groupId && m.UserId == userId);
        if (!isMember)
        {
            throw new NotFoundException(nameof(StudyGroup), groupId);
        }

        return db.StudyGroups.FirstOrDefault(g => g.Id == groupId)
            ?? throw new NotFoundException(nameof(StudyGroup), groupId);
    }

    private Dictionary<Guid, string> SchoolNamesById(IEnumerable<Guid> schoolIds)
    {
        var ids = schoolIds.Distinct().ToList();
        return db.Schools.Where(s => ids.Contains(s.Id)).ToList().ToDictionary(s => s.Id, s => s.Name);
    }

    private Dictionary<Guid, int> MemberCountsByGroup(IEnumerable<Guid> groupIds)
    {
        var ids = groupIds.Distinct().ToList();
        return db.StudyGroupMembers.Where(m => ids.Contains(m.StudyGroupId)).ToList()
            .GroupBy(m => m.StudyGroupId)
            .ToDictionary(g => g.Key, g => g.Count());
    }

    private static StudyGroupDto ToDto(
        StudyGroup g, Guid userId, IReadOnlyDictionary<Guid, string> schoolNamesById, IReadOnlyDictionary<Guid, int> memberCountsByGroup) => new(
        g.Id,
        g.Name,
        g.Subject,
        g.InviteCode,
        schoolNamesById.GetValueOrDefault(g.SchoolId, "Unknown school"),
        g.OwnerUserId,
        g.OwnerUserId == userId,
        memberCountsByGroup.GetValueOrDefault(g.Id, 0),
        g.CreatedAt);
}
