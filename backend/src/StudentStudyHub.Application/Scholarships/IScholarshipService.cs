namespace StudentStudyHub.Application.Scholarships;

public interface IScholarshipService
{
    Task<IReadOnlyList<ScholarshipDto>> ListAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<ScholarshipDto> GetAsync(Guid userId, Guid scholarshipId, CancellationToken cancellationToken = default);
    Task<ScholarshipDto> CreateAsync(Guid userId, CreateScholarshipRequest request, CancellationToken cancellationToken = default);
    Task<ScholarshipDto> UpdateAsync(Guid userId, Guid scholarshipId, UpdateScholarshipRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid userId, Guid scholarshipId, CancellationToken cancellationToken = default);
}
