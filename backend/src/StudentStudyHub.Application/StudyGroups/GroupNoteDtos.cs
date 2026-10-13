namespace StudentStudyHub.Application.StudyGroups;

public record GroupNoteDto(
    Guid Id,
    Guid StudyGroupId,
    Guid AuthorUserId,
    string AuthorName,
    bool IsMine,
    string Title,
    string? Content,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public record CreateGroupNoteRequest(string Title, string? Content);

public record UpdateGroupNoteRequest(string Title, string? Content);
