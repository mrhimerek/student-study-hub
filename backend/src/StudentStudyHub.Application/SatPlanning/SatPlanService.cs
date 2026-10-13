using StudentStudyHub.Application.Common.Interfaces;
using StudentStudyHub.Domain.Entities;
using StudentStudyHub.Domain.Exceptions;

namespace StudentStudyHub.Application.SatPlanning;

/// <summary>
/// CRUD for the SAT study planner. SatPlan is a single-record-per-user resource (no plan id is
/// ever passed in from the caller - everything is looked up/created by UserId), with a nested
/// list of practice scores scoped by SatPlanId. Mirrors ExamService/ScholarshipService for the
/// ownership-scoping style, but shaped around "at most one plan" instead of a list.
/// </summary>
public class SatPlanService(IAppDbContext db, IDateTimeProvider clock) : ISatPlanService
{
    public Task<SatPlanDto?> GetAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var plan = db.SatPlans.FirstOrDefault(p => p.UserId == userId);
        return Task.FromResult(plan is null ? null : ToDto(plan));
    }

    public async Task<SatPlanDto> CreateAsync(Guid userId, CreateSatPlanRequest request, CancellationToken cancellationToken = default)
    {
        if (db.SatPlans.Any(p => p.UserId == userId))
        {
            throw new DomainException("You already have an SAT plan. Update it instead of creating a new one.");
        }

        var plan = new SatPlan
        {
            UserId = userId,
            TargetTestDate = request.TargetTestDate,
            GoalScore = request.GoalScore,
        };

        db.Add(plan);
        await db.SaveChangesAsync(cancellationToken);

        return ToDto(plan);
    }

    public async Task<SatPlanDto> UpdateAsync(Guid userId, UpdateSatPlanRequest request, CancellationToken cancellationToken = default)
    {
        var plan = FindOwned(userId);

        plan.TargetTestDate = request.TargetTestDate;
        plan.GoalScore = request.GoalScore;
        plan.UpdatedAt = clock.UtcNow;

        await db.SaveChangesAsync(cancellationToken);

        return ToDto(plan);
    }

    public async Task DeleteAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var plan = FindOwned(userId);
        db.Remove(plan);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<SatPlanDto> LogPracticeScoreAsync(Guid userId, LogSatPracticeScoreRequest request, CancellationToken cancellationToken = default)
    {
        var plan = FindOwned(userId);

        var score = new SatPracticeScore
        {
            SatPlanId = plan.Id,
            TestDate = request.TestDate,
            ReadingWritingScore = request.ReadingWritingScore,
            MathScore = request.MathScore,
        };

        db.Add(score);
        await db.SaveChangesAsync(cancellationToken);

        return ToDto(plan);
    }

    public async Task<SatPlanDto> DeletePracticeScoreAsync(Guid userId, Guid scoreId, CancellationToken cancellationToken = default)
    {
        var plan = FindOwned(userId);
        var score = db.SatPracticeScores.FirstOrDefault(s => s.Id == scoreId && s.SatPlanId == plan.Id)
            ?? throw new NotFoundException(nameof(SatPracticeScore), scoreId);

        db.Remove(score);
        await db.SaveChangesAsync(cancellationToken);

        return ToDto(plan);
    }

    private SatPlan FindOwned(Guid userId)
    {
        return db.SatPlans.FirstOrDefault(p => p.UserId == userId)
            ?? throw new NotFoundException(nameof(SatPlan), userId);
    }

    private SatPlanDto ToDto(SatPlan plan)
    {
        var scores = db.SatPracticeScores
            .Where(s => s.SatPlanId == plan.Id)
            .OrderBy(s => s.TestDate)
            .Select(ToScoreDto)
            .ToList();

        return new SatPlanDto(plan.Id, plan.TargetTestDate, plan.GoalScore, scores, plan.CreatedAt, plan.UpdatedAt);
    }

    private static SatPracticeScoreDto ToScoreDto(SatPracticeScore s) => new(
        s.Id, s.TestDate, s.ReadingWritingScore, s.MathScore, s.ReadingWritingScore + s.MathScore, s.CreatedAt);
}
