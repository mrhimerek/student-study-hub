namespace StudentStudyHub.Application.StudyGroups;

public interface IStudyGroupService
{
    Task<IReadOnlyList<StudyGroupDto>> ListMineAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<StudyGroupDetailDto> GetAsync(Guid userId, Guid groupId, CancellationToken cancellationToken = default);
    Task<StudyGroupDto> CreateAsync(Guid userId, CreateStudyGroupRequest request, CancellationToken cancellationToken = default);
    Task<StudyGroupDto> UpdateAsync(Guid userId, Guid groupId, UpdateStudyGroupRequest request, CancellationToken cancellationToken = default);
    Task<StudyGroupDto> JoinAsync(Guid userId, JoinStudyGroupRequest request, CancellationToken cancellationToken = default);
    Task LeaveAsync(Guid userId, Guid groupId, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid userId, Guid groupId, CancellationToken cancellationToken = default);
}
