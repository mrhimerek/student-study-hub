using StudentStudyHub.Application.Common.Interfaces;
using StudentStudyHub.Application.Courses;
using StudentStudyHub.Domain.Entities;
using StudentStudyHub.Domain.Exceptions;

namespace StudentStudyHub.Application.UnitTests.Courses;

public class CourseServiceTests
{
    private static readonly Guid UserId = Guid.NewGuid();
    private static readonly Guid OtherUserId = Guid.NewGuid();

    private static (CourseService Service, FakeAppDbContext Db, FakeDateTimeProvider Clock) CreateSut()
    {
        var db = new FakeAppDbContext();
        var clock = new FakeDateTimeProvider();
        var service = new CourseService(db, clock);
        return (service, db, clock);
    }

    private static CreateCourseRequest ValidCreateRequest() =>
        new("Algebra II", 1.0m, "A", false, "Fall 2025");

    [Fact]
    public async Task CreateAsync_PersistsCourse()
    {
        var (service, db, _) = CreateSut();

        var created = await service.CreateAsync(UserId, ValidCreateRequest());

        Assert.Single(db.CoursesList);
        Assert.Equal("Algebra II", created.Name);
        Assert.Equal(UserId, db.CoursesList[0].UserId);
    }

    [Fact]
    public async Task ListAsync_OnlyReturnsCoursesOwnedByUser_OrderedByTermThenName()
    {
        var (service, _, _) = CreateSut();
        await service.CreateAsync(UserId, ValidCreateRequest() with { Name = "Zoology", Term = "Fall 2025" });
        await service.CreateAsync(UserId, ValidCreateRequest() with { Name = "Algebra", Term = "Fall 2025" });
        await service.CreateAsync(OtherUserId, ValidCreateRequest() with { Name = "NotMine" });

        var result = await service.ListAsync(UserId);

        Assert.Equal(2, result.Count);
        Assert.Equal("Algebra", result[0].Name);
        Assert.Equal("Zoology", result[1].Name);
    }

    [Fact]
    public async Task GetAsync_ForCourseOwnedByAnotherUser_ThrowsNotFound()
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
            new UpdateCourseRequest("AP Algebra II", 1.0m, "A+", true, "Fall 2025"));

        Assert.Equal("AP Algebra II", updated.Name);
        Assert.True(updated.IsHonorsOrAp);
        Assert.NotNull(updated.UpdatedAt);
    }

    [Fact]
    public async Task UpdateAsync_ForUnknownCourse_ThrowsNotFound()
    {
        var (service, _, _) = CreateSut();

        await Assert.ThrowsAsync<NotFoundException>(() =>
            service.UpdateAsync(UserId, Guid.NewGuid(), new UpdateCourseRequest("X", 1.0m, "A", false, "Fall 2025")));
    }

    [Fact]
    public async Task DeleteAsync_RemovesCourse()
    {
        var (service, db, _) = CreateSut();
        var created = await service.CreateAsync(UserId, ValidCreateRequest());

        await service.DeleteAsync(UserId, created.Id);

        Assert.Empty(db.CoursesList);
    }

    [Fact]
    public async Task DeleteAsync_ForCourseOwnedByAnotherUser_ThrowsNotFound_AndDoesNotDelete()
    {
        var (service, db, _) = CreateSut();
        var created = await service.CreateAsync(OtherUserId, ValidCreateRequest());

        await Assert.ThrowsAsync<NotFoundException>(() => service.DeleteAsync(UserId, created.Id));
        Assert.Single(db.CoursesList);
    }

    private class FakeAppDbContext : IAppDbContext
    {
        public List<Course> CoursesList { get; } = new();

        public IQueryable<School> Schools => new List<School>().AsQueryable();
        public IQueryable<User> Users => new List<User>().AsQueryable();
        public IQueryable<Assignment> Assignments => new List<Assignment>().AsQueryable();
        public IQueryable<StudyGroup> StudyGroups => new List<StudyGroup>().AsQueryable();
        public IQueryable<StudyGroupMember> StudyGroupMembers => new List<StudyGroupMember>().AsQueryable();
        public IQueryable<GroupNote> GroupNotes => new List<GroupNote>().AsQueryable();
        public IQueryable<Exam> Exams => new List<Exam>().AsQueryable();
        public IQueryable<Course> Courses => CoursesList.AsQueryable();
        public IQueryable<SatPlan> SatPlans => new List<SatPlan>().AsQueryable();
        public IQueryable<SatPracticeScore> SatPracticeScores => new List<SatPracticeScore>().AsQueryable();
        public IQueryable<Scholarship> Scholarships => new List<Scholarship>().AsQueryable();
        public IQueryable<CollegeApplication> CollegeApplications => new List<CollegeApplication>().AsQueryable();
        public IQueryable<Notification> Notifications => new List<Notification>().AsQueryable();
        public IQueryable<RefreshToken> RefreshTokens => new List<RefreshToken>().AsQueryable();

        public void Add<TEntity>(TEntity entity) where TEntity : class
        {
            if (entity is Course course)
            {
                CoursesList.Add(course);
                return;
            }

            throw new NotSupportedException($"FakeAppDbContext does not support adding {typeof(TEntity).Name}.");
        }

        public void Remove<TEntity>(TEntity entity) where TEntity : class
        {
            if (entity is Course course)
            {
                CoursesList.Remove(course);
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
