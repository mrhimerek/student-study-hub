namespace StudentStudyHub.Application.SatPlanning;

/// <summary>
/// One active SAT plan per user: a target test date + goal score, plus a log of practice test
/// scores over time. Unlike Exams/Scholarships/CollegeApplications (lists of many owned rows),
/// this is a single-record-per-user resource - GetAsync returns null (not a 404) when the user
/// hasn't created a plan yet, mirroring how GpaSummary always returns a value for "no data yet".
/// </summary>
public interface ISatPlanService
{
    Task<SatPlanDto?> GetAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<SatPlanDto> CreateAsync(Guid userId, CreateSatPlanRequest request, CancellationToken cancellationToken = default);

    Task<SatPlanDto> UpdateAsync(Guid userId, UpdateSatPlanRequest request, CancellationToken cancellationToken = default);

    Task DeleteAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<SatPlanDto> LogPracticeScoreAsync(Guid userId, LogSatPracticeScoreRequest request, CancellationToken cancellationToken = default);

    Task<SatPlanDto> DeletePracticeScoreAsync(Guid userId, Guid scoreId, CancellationToken cancellationToken = default);
}
