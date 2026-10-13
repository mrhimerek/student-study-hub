namespace StudentStudyHub.Application.StudyGroups;

public interface IGroupNoteService
{
    Task<IReadOnlyList<GroupNoteDto>> ListAsync(Guid userId, Guid groupId, CancellationToken cancellationToken = default);
    Task<GroupNoteDto> CreateAsync(Guid userId, Guid groupId, CreateGroupNoteRequest request, CancellationToken cancellationToken = default);
    Task<GroupNoteDto> UpdateAsync(Guid userId, Guid groupId, Guid noteId, UpdateGroupNoteRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid userId, Guid groupId, Guid noteId, CancellationToken cancellationToken = default);
}
