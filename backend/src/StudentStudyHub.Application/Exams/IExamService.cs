namespace StudentStudyHub.Application.Exams;

public interface IExamService
{
    Task<IReadOnlyList<ExamDto>> ListAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<ExamDto?> GetNextAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<ExamDto> GetAsync(Guid userId, Guid examId, CancellationToken cancellationToken = default);
    Task<ExamDto> CreateAsync(Guid userId, CreateExamRequest request, CancellationToken cancellationToken = default);
    Task<ExamDto> UpdateAsync(Guid userId, Guid examId, UpdateExamRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid userId, Guid examId, CancellationToken cancellationToken = default);
}
