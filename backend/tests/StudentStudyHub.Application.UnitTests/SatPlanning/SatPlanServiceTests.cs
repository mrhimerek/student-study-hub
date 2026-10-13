using StudentStudyHub.Application.Common.Interfaces;
using StudentStudyHub.Application.SatPlanning;
using StudentStudyHub.Domain.Entities;
using StudentStudyHub.Domain.Exceptions;

namespace StudentStudyHub.Application.UnitTests.SatPlanning;

public class SatPlanServiceTests
{
    private static readonly Guid UserId = Guid.NewGuid();
    private static readonly Guid OtherUserId = Guid.NewGuid();

    private static (SatPlanService Service, FakeAppDbContext Db, FakeDateTimeProvider Clock) CreateSut()
    {
        var db = new FakeAppDbContext();
        var clock = new FakeDateTimeProvider();
        var service = new SatPlanService(db, clock);
        return (service, db, clock);
    }

    private static CreateSatPlanRequest ValidCreateRequest() => new(new DateOnly(2026, 5, 2), 1450);

    [Fact]
    public async Task GetAsync_WhenNoPlanExists_ReturnsNull()
    {
        var (service, _, _) = CreateSut();

        var result = await service.GetAsync(UserId);

        Assert.Null(result);
    }

    [Fact]
    public async Task CreateAsync_PersistsPlan()
    {
        var (service, db, _) = CreateSut();

        var created = await service.CreateAsync(UserId, ValidCreateRequest());

        Assert.Single(db.SatPlansList);
        Assert.Equal(UserId, db.SatPlansList[0].UserId);
        Assert.Equal(1450, created.GoalScore);
        Assert.Empty(created.PracticeScores);
    }

    [Fact]
    public async Task CreateAsync_WhenPlanAlreadyExists_ThrowsDomainException()
    {
        var (service, _, _) = CreateSut();
        await service.CreateAsync(UserId, ValidCreateRequest());

        await Assert.ThrowsAsync<DomainException>(() => service.CreateAsync(UserId, ValidCreateRequest()));
    }

    [Fact]
    public async Task CreateAsync_ForDifferentUsers_DoesNotConflict()
    {
        var (service, db, _) = CreateSut();
        await service.CreateAsync(UserId, ValidCreateRequest());

        await service.CreateAsync(OtherUserId, ValidCreateRequest());

        Assert.Equal(2, db.SatPlansList.Count);
    }

    [Fact]
    public async Task GetAsync_ReturnsScoresOrderedByTestDate()
    {
        var (service, _, _) = CreateSut();
        await service.CreateAsync(UserId, ValidCreateRequest());
        await service.LogPracticeScoreAsync(UserId, new LogSatPracticeScoreRequest(new DateOnly(2026, 3, 1), 650, 620));
        await service.LogPracticeScoreAsync(UserId, new LogSatPracticeScoreRequest(new DateOnly(2026, 1, 1), 600, 580));

        var plan = await service.GetAsync(UserId);

        Assert.NotNull(plan);
        Assert.Equal(2, plan!.PracticeScores.Count);
        Assert.Equal(new DateOnly(2026, 1, 1), plan.PracticeScores[0].TestDate);
        Assert.Equal(new DateOnly(2026, 3, 1), plan.PracticeScores[1].TestDate);
        Assert.Equal(1270, plan.PracticeScores[1].TotalScore);
    }

    [Fact]
    public async Task UpdateAsync_UpdatesFields_AndSetsUpdatedAt()
    {
        var (service, _, clock) = CreateSut();
        await service.CreateAsync(UserId, ValidCreateRequest());
        clock.UtcNow = clock.UtcNow.AddHours(1);

        var updated = await service.UpdateAsync(UserId, new UpdateSatPlanRequest(new DateOnly(2026, 8, 1), 1500));

        Assert.Equal(new DateOnly(2026, 8, 1), updated.TargetTestDate);
        Assert.Equal(1500, updated.GoalScore);
        Assert.NotNull(updated.UpdatedAt);
    }

    [Fact]
    public async Task UpdateAsync_WhenNoPlanExists_ThrowsNotFound()
    {
        var (service, _, _) = CreateSut();

        await Assert.ThrowsAsync<NotFoundException>(() =>
            service.UpdateAsync(UserId, new UpdateSatPlanRequest(new DateOnly(2026, 8, 1), 1500)));
    }

    [Fact]
    public async Task DeleteAsync_RemovesPlan()
    {
        var (service, db, _) = CreateSut();
        await service.CreateAsync(UserId, ValidCreateRequest());

        await service.DeleteAsync(UserId);

        Assert.Empty(db.SatPlansList);
    }

    [Fact]
    public async Task DeleteAsync_WhenNoPlanExists_ThrowsNotFound()
    {
        var (service, _, _) = CreateSut();

        await Assert.ThrowsAsync<NotFoundException>(() => service.DeleteAsync(UserId));
    }

    [Fact]
    public async Task LogPracticeScoreAsync_AddsScore_ReturnsUpdatedPlan()
    {
        var (service, db, _) = CreateSut();
        await service.CreateAsync(UserId, ValidCreateRequest());

        var updated = await service.LogPracticeScoreAsync(UserId, new LogSatPracticeScoreRequest(new DateOnly(2026, 2, 1), 700, 710));

        Assert.Single(db.SatPracticeScoresList);
        Assert.Single(updated.PracticeScores);
        Assert.Equal(1410, updated.PracticeScores[0].TotalScore);
    }

    [Fact]
    public async Task LogPracticeScoreAsync_WhenNoPlanExists_ThrowsNotFound()
    {
        var (service, _, _) = CreateSut();

        await Assert.ThrowsAsync<NotFoundException>(() =>
            service.LogPracticeScoreAsync(UserId, new LogSatPracticeScoreRequest(new DateOnly(2026, 2, 1), 700, 710)));
    }

    [Fact]
    public async Task DeletePracticeScoreAsync_RemovesScore()
    {
        var (service, db, _) = CreateSut();
        await service.CreateAsync(UserId, ValidCreateRequest());
        var afterLog = await service.LogPracticeScoreAsync(UserId, new LogSatPracticeScoreRequest(new DateOnly(2026, 2, 1), 700, 710));
        var scoreId = afterLog.PracticeScores[0].Id;

        var updated = await service.DeletePracticeScoreAsync(UserId, scoreId);

        Assert.Empty(db.SatPracticeScoresList);
        Assert.Empty(updated.PracticeScores);
    }

    [Fact]
    public async Task DeletePracticeScoreAsync_ForUnknownScore_ThrowsNotFound()
    {
        var (service, _, _) = CreateSut();
        await service.CreateAsync(UserId, ValidCreateRequest());

        await Assert.ThrowsAsync<NotFoundException>(() => service.DeletePracticeScoreAsync(UserId, Guid.NewGuid()));
    }

    private class FakeAppDbContext : IAppDbContext
    {
        public List<SatPlan> SatPlansList { get; } = new();
        public List<SatPracticeScore> SatPracticeScoresList { get; } = new();

        public IQueryable<School> Schools => new List<School>().AsQueryable();
        public IQueryable<User> Users => new List<User>().AsQueryable();
        public IQueryable<Assignment> Assignments => new List<Assignment>().AsQueryable();
        public IQueryable<StudyGroup> StudyGroups => new List<StudyGroup>().AsQueryable();
        public IQueryable<StudyGroupMember> StudyGroupMembers => new List<StudyGroupMember>().AsQueryable();
        public IQueryable<GroupNote> GroupNotes => new List<GroupNote>().AsQueryable();
        public IQueryable<Exam> Exams => new List<Exam>().AsQueryable();
        public IQueryable<Course> Courses => new List<Course>().AsQueryable();
        public IQueryable<SatPlan> SatPlans => SatPlansList.AsQueryable();
        public IQueryable<SatPracticeScore> SatPracticeScores => SatPracticeScoresList.AsQueryable();
        public IQueryable<Scholarship> Scholarships => new List<Scholarship>().AsQueryable();
        public IQueryable<CollegeApplication> CollegeApplications => new List<CollegeApplication>().AsQueryable();
        public IQueryable<Notification> Notifications => new List<Notification>().AsQueryable();
        public IQueryable<RefreshToken> RefreshTokens => new List<RefreshToken>().AsQueryable();

        public void Add<TEntity>(TEntity entity) where TEntity : class
        {
            switch (entity)
            {
                case SatPlan plan:
                    SatPlansList.Add(plan);
                    return;
                case SatPracticeScore score:
                    SatPracticeScoresList.Add(score);
                    return;
                default:
                    throw new NotSupportedException($"FakeAppDbContext does not support adding {typeof(TEntity).Name}.");
            }
        }

        public void Remove<TEntity>(TEntity entity) where TEntity : class
        {
            switch (entity)
            {
                case SatPlan plan:
                    SatPlansList.Remove(plan);
                    return;
                case SatPracticeScore score:
                    SatPracticeScoresList.Remove(score);
                    return;
                default:
                    throw new NotSupportedException($"FakeAppDbContext does not support removing {typeof(TEntity).Name}.");
            }
        }

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => Task.FromResult(0);
    }

    private class FakeDateTimeProvider : IDateTimeProvider
    {
        public DateTime UtcNow { get; set; } = DateTime.UtcNow;
    }
}
