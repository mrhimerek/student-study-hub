using StudentStudyHub.Domain.Enums;

namespace StudentStudyHub.Application.Assignments;

public interface IAssignmentService
{
    Task<IReadOnlyList<AssignmentDto>> ListAsync(Guid userId, AssignmentStatus? status, CancellationToken cancellationToken = default);
    Task<AssignmentDto> GetAsync(Guid userId, Guid assignmentId, CancellationToken cancellationToken = default);
    Task<AssignmentDto> CreateAsync(Guid userId, CreateAssignmentRequest request, CancellationToken cancellationToken = default);
    Task<AssignmentDto> UpdateAsync(Guid userId, Guid assignmentId, UpdateAssignmentRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid userId, Guid assignmentId, CancellationToken cancellationToken = default);
}
