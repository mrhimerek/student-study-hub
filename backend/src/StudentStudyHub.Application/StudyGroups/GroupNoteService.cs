using StudentStudyHub.Application.Common.Interfaces;
using StudentStudyHub.Domain.Entities;
using StudentStudyHub.Domain.Exceptions;

namespace StudentStudyHub.Application.StudyGroups;

/// <summary>
/// CRUD for notes shared within a study group. Every operation first checks the caller is a
/// member of the group (NotFoundException if not, so non-members can't probe group/note ids),
/// then authors can edit/delete their own notes and the group owner can delete any note
/// (light moderation) - editing someone else's note is always forbidden.
/// </summary>
public class GroupNoteService(IAppDbContext db, IDateTimeProvider clock) : IGroupNoteService
{
    public Task<IReadOnlyList<GroupNoteDto>> ListAsync(Guid userId, Guid groupId, CancellationToken cancellationToken = default)
    {
        EnsureMember(userId, groupId);

        var notes = db.GroupNotes.Where(n => n.StudyGroupId == groupId).OrderByDescending(n => n.CreatedAt).ToList();
        var authorsById = db.Users.Where(u => notes.Select(n => n.AuthorUserId).Contains(u.Id)).ToList().ToDictionary(u => u.Id);

        IReadOnlyList<GroupNoteDto> result = notes.Select(n => ToDto(n, userId, authorsById)).ToList();
        return Task.FromResult(result);
    }

    public async Task<GroupNoteDto> CreateAsync(Guid userId, Guid groupId, CreateGroupNoteRequest request, CancellationToken cancellationToken = default)
    {
        EnsureMember(userId, groupId);

        var note = new GroupNote
        {
            StudyGroupId = groupId,
            AuthorUserId = userId,
            Title = request.Title.Trim(),
            Content = string.IsNullOrWhiteSpace(request.Content) ? null : request.Content.Trim(),
        };

        db.Add(note);
        await db.SaveChangesAsync(cancellationToken);

        var author = db.Users.FirstOrDefault(u => u.Id == userId);
        return ToDto(note, userId, author is null ? [] : new Dictionary<Guid, User> { [userId] = author });
    }

    public async Task<GroupNoteDto> UpdateAsync(Guid userId, Guid groupId, Guid noteId, UpdateGroupNoteRequest request, CancellationToken cancellationToken = default)
    {
        EnsureMember(userId, groupId);
        var note = FindNoteInGroup(groupId, noteId);

        if (note.AuthorUserId != userId)
        {
            throw new ForbiddenException("Only the author can edit this note.");
        }

        note.Title = request.Title.Trim();
        note.Content = string.IsNullOrWhiteSpace(request.Content) ? null : request.Content.Trim();
        note.UpdatedAt = clock.UtcNow;

        await db.SaveChangesAsync(cancellationToken);

        var author = db.Users.FirstOrDefault(u => u.Id == userId);
        return ToDto(note, userId, author is null ? [] : new Dictionary<Guid, User> { [userId] = author });
    }

    public async Task DeleteAsync(Guid userId, Guid groupId, Guid noteId, CancellationToken cancellationToken = default)
    {
        EnsureMember(userId, groupId);
        var note = FindNoteInGroup(groupId, noteId);

        var group = db.StudyGroups.FirstOrDefault(g => g.Id == groupId) ?? throw new NotFoundException(nameof(StudyGroup), groupId);
        var isGroupOwner = group.OwnerUserId == userId;
        if (note.AuthorUserId != userId && !isGroupOwner)
        {
            throw new ForbiddenException("Only the author or the group owner can delete this note.");
        }

        db.Remove(note);
        await db.SaveChangesAsync(cancellationToken);
    }

    private void EnsureMember(Guid userId, Guid groupId)
    {
        var isMember = db.StudyGroupMembers.Any(m => m.StudyGroupId == groupId && m.UserId == userId);
        if (!isMember)
        {
            throw new NotFoundException(nameof(StudyGroup), groupId);
        }
    }

    private GroupNote FindNoteInGroup(Guid groupId, Guid noteId)
    {
        return db.GroupNotes.FirstOrDefault(n => n.Id == noteId && n.StudyGroupId == groupId)
            ?? throw new NotFoundException(nameof(GroupNote), noteId);
    }

    private static GroupNoteDto ToDto(GroupNote n, Guid userId, IReadOnlyDictionary<Guid, User> authorsById)
    {
        var author = authorsById.GetValueOrDefault(n.AuthorUserId);
        var authorName = author is null ? "Unknown" : $"{author.FirstName} {author.LastName}".Trim();
        return new GroupNoteDto(n.Id, n.StudyGroupId, n.AuthorUserId, authorName, n.AuthorUserId == userId, n.Title, n.Content, n.CreatedAt, n.UpdatedAt);
    }
}
