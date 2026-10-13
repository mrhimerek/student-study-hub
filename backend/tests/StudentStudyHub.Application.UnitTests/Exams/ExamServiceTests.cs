using StudentStudyHub.Application.Common.Interfaces;
using StudentStudyHub.Application.Exams;
using StudentStudyHub.Domain.Entities;
using StudentStudyHub.Domain.Enums;
using StudentStudyHub.Domain.Exceptions;

namespace StudentStudyHub.Application.UnitTests.Exams;

public class ExamServiceTests
{
    private static readonly Guid UserId = Guid.NewGuid();
    private static readonly Guid OtherUserId = Guid.NewGuid();

    private static (ExamService Service, FakeAppDbContext Db, FakeDateTimeProvider Clock) CreateSut()
    {
        var db = new FakeAppDbContext();
        var clock = new FakeDateTimeProvider();
        var service = new ExamService(db, clock);
        return (service, db, clock);
    }

    private static CreateExamRequest ValidCreateRequest() =>
        new("Biology", "Final exam", DateTime.UtcNow.AddDays(3), ExamType.Final, "Chapters 1-10");

    [Fact]
    public async Task CreateAsync_PersistsExam()
    {
        var (service, db, _) = CreateSut();

        var created = await service.CreateAsync(UserId, ValidCreateRequest());

        Assert.Single(db.ExamsList);
        Assert.Equal("Biology", created.Subject);
        Assert.Equal(UserId, db.ExamsList[0].UserId);
    }

    [Fact]
    public async Task ListAsync_OnlyReturnsExamsOwnedByUser_OrderedByExamDate()
    {
        var (service, _, _) = CreateSut();
        await service.CreateAsync(UserId, ValidCreateRequest() with { Title = "Later", ExamDate = DateTime.UtcNow.AddDays(10) });
        await service.CreateAsync(UserId, ValidCreateRequest() with { Title = "Sooner", ExamDate = DateTime.UtcNow.AddDays(1) });
        await service.CreateAsync(OtherUserId, ValidCreateRequest() with { Title = "NotMine" });

        var result = await service.ListAsync(UserId);

        Assert.Equal(2, result.Count);
        Assert.Equal("Sooner", result[0].Title);
        Assert.Equal("Later", result[1].Title);
    }

    [Fact]
    public async Task GetNextAsync_ReturnsSoonestFutureExam_IgnoringPastExamsAndOtherUsers()
    {
        var (service, _, _) = CreateSut();
        await service.CreateAsync(UserId, ValidCreateRequest() with { Title = "Past", ExamDate = DateTime.UtcNow.AddDays(-1) });
        await service.CreateAsync(UserId, ValidCreateRequest() with { Title = "Later", ExamDate = DateTime.UtcNow.AddDays(10) });
        await service.CreateAsync(UserId, ValidCreateRequest() with { Title = "Sooner", ExamDate = DateTime.UtcNow.AddDays(2) });
        await service.CreateAsync(OtherUserId, ValidCreateRequest() with { Title = "NotMine", ExamDate = DateTime.UtcNow.AddHours(1) });

        var next = await service.GetNextAsync(UserId);

        Assert.NotNull(next);
        Assert.Equal("Sooner", next!.Title);
    }

    [Fact]
    public async Task GetNextAsync_WithNoUpcomingExams_ReturnsNull()
    {
        var (service, _, _) = CreateSut();
        await service.CreateAsync(UserId, ValidCreateRequest() with { ExamDate = DateTime.UtcNow.AddDays(-1) });

        var next = await service.GetNextAsync(UserId);

        Assert.Null(next);
    }

    [Fact]
    public async Task GetAsync_ForExamOwnedByAnotherUser_ThrowsNotFound()
    {
        var (service, _, _) = CreateSut();
        var created = await service.CreateAsync(OtherUserId, ValidCreateRequest());

        await Assert.ThrowsAsync<NotFoundException>(() => service.GetAsync(UserId, created.Id));
    }

    [Fact]
    public async Task UpdateAsync_UpdatesFields_AndSetsUpdatedAt()
    {
        var (service, _, clock) = CreateSut();
        var created = await service.CreateAsync(UserId, ValidCreateRequest());
        clock.UtcNow = clock.UtcNow.AddHours(1);

        var updated = await service.UpdateAsync(
            UserId,
            created.Id,
            new UpdateExamRequest("Chemistry", "Midterm", created.ExamDate.AddDays(1), ExamType.Exam, "Updated notes"));

        Assert.Equal("Chemistry", updated.Subject);
        Assert.Equal(ExamType.Exam, updated.Type);
        Assert.NotNull(updated.UpdatedAt);
    }

    [Fact]
    public async Task UpdateAsync_ForUnknownExam_ThrowsNotFound()
    {
        var (service, _, _) = CreateSut();

        await Assert.ThrowsAsync<NotFoundException>(() =>
            service.UpdateAsync(UserId, Guid.NewGuid(), new UpdateExamRequest("X", "Y", DateTime.UtcNow, ExamType.Quiz, null)));
    }

    [Fact]
    public async Task DeleteAsync_RemovesExam()
    {
        var (service, db, _) = CreateSut();
        var created = await service.CreateAsync(UserId, ValidCreateRequest());

        await service.DeleteAsync(UserId, created.Id);

        Assert.Empty(db.ExamsList);
    }

    [Fact]
    public async Task DeleteAsync_ForExamOwnedByAnotherUser_ThrowsNotFound_AndDoesNotDelete()
    {
        var (service, db, _) = CreateSut();
        var created = await service.CreateAsync(OtherUserId, ValidCreateRequest());

        await Assert.ThrowsAsync<NotFoundException>(() => service.DeleteAsync(UserId, created.Id));
        Assert.Single(db.ExamsList);
    }

    private class FakeAppDbContext : IAppDbContext
    {
        public List<Exam> ExamsList { get; } = new();

        public IQueryable<School> Schools => new List<School>().AsQueryable();
        public IQueryable<User> Users => new List<User>().AsQueryable();
        public IQueryable<Assignment> Assignments => new List<Assignment>().AsQueryable();
        public IQueryable<StudyGroup> StudyGroups => new List<StudyGroup>().AsQueryable();
        public IQueryable<StudyGroupMember> StudyGroupMembers => new List<StudyGroupMember>().AsQueryable();
        public IQueryable<GroupNote> GroupNotes => new List<GroupNote>().AsQueryable();
        public IQueryable<Exam> Exams => ExamsList.AsQueryable();
        public IQueryable<Course> Courses => new List<Course>().AsQueryable();
        public IQueryable<SatPlan> SatPlans => new List<SatPlan>().AsQueryable();
        public IQueryable<SatPracticeScore> SatPracticeScores => new List<SatPracticeScore>().AsQueryable();
        public IQueryable<Scholarship> Scholarships => new List<Scholarship>().AsQueryable();
        public IQueryable<CollegeApplication> CollegeApplications => new List<CollegeApplication>().AsQueryable();
        public IQueryable<Notification> Notifications => new List<Notification>().AsQueryable();
        public IQueryable<RefreshToken> RefreshTokens => new List<RefreshToken>().AsQueryable();

        public void Add<TEntity>(TEntity entity) where TEntity : class
        {
            if (entity is Exam exam)
            {
                ExamsList.Add(exam);
                return;
            }

            throw new NotSupportedException($"FakeAppDbContext does not support adding {typeof(TEntity).Name}.");
        }

        public void Remove<TEntity>(TEntity entity) where TEntity : class
        {
            if (entity is Exam exam)
            {
                ExamsList.Remove(exam);
                return;
            }

            throw new NotSupportedException($"FakeAppDbContext does not support removing {typeof(TEntity).Name}.");
        }

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => Task.FromResult(0);
    }

    private class FakeDateTimeProvider : IDateTimeProvider
    {
        public DateTime UtcNow { get; set; } = DateTime.UtcNow;
    }
}
