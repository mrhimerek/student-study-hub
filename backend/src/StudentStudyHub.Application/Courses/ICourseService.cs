namespace StudentStudyHub.Application.Courses;

public interface ICourseService
{
    Task<IReadOnlyList<CourseDto>> ListAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<CourseDto> GetAsync(Guid userId, Guid courseId, CancellationToken cancellationToken = default);
    Task<CourseDto> CreateAsync(Guid userId, CreateCourseRequest request, CancellationToken cancellationToken = default);
    Task<CourseDto> UpdateAsync(Guid userId, Guid courseId, UpdateCourseRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid userId, Guid courseId, CancellationToken cancellationToken = default);
}
