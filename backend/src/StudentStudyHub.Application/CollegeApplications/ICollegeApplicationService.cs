namespace StudentStudyHub.Application.CollegeApplications;

public interface ICollegeApplicationService
{
    Task<IReadOnlyList<CollegeApplicationDto>> ListAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<CollegeApplicationDto> GetAsync(Guid userId, Guid applicationId, CancellationToken cancellationToken = default);
    Task<CollegeApplicationDto> CreateAsync(Guid userId, CreateCollegeApplicationRequest request, CancellationToken cancellationToken = default);
    Task<CollegeApplicationDto> UpdateAsync(Guid userId, Guid applicationId, UpdateCollegeApplicationRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid userId, Guid applicationId, CancellationToken cancellationToken = default);
}
