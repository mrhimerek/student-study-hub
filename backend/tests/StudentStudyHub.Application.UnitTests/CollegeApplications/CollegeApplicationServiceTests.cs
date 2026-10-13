using StudentStudyHub.Application.CollegeApplications;
using StudentStudyHub.Application.Common.Interfaces;
using StudentStudyHub.Domain.Entities;
using StudentStudyHub.Domain.Enums;
using StudentStudyHub.Domain.Exceptions;

namespace StudentStudyHub.Application.UnitTests.CollegeApplications;

public class CollegeApplicationServiceTests
{
    private static readonly Guid UserId = Guid.NewGuid();
    private static readonly Guid OtherUserId = Guid.NewGuid();

    private static (CollegeApplicationService Service, FakeAppDbContext Db, FakeDateTimeProvider Clock) CreateSut()
    {
        var db = new FakeAppDbContext();
        var clock = new FakeDateTimeProvider();
        var service = new CollegeApplicationService(db, clock);
        return (service, db, clock);
    }

    private static CreateCollegeApplicationRequest ValidCreateRequest() =>
        new("State University", ApplicationType.EarlyAction, new DateOnly(2026, 11, 1), CollegeApplicationStatus.NotStarted, ["Transcript", "Essay"]);

    [Fact]
    public async Task CreateAsync_PersistsApplication_WithRequirements()
    {
        var (service, db, _) = CreateSut();

        var created = await service.CreateAsync(UserId, ValidCreateRequest());

        Assert.Single(db.CollegeApplicationsList);
        Assert.Equal("State University", created.CollegeName);
        Assert.Equal(UserId, db.CollegeApplicationsList[0].UserId);
        Assert.Equal(2, created.Requirements.Count);
    }

    [Fact]
    public async Task CreateAsync_WithNoRequirements_ReturnsEmptyList()
    {
        var (service, _, _) = CreateSut();

        var created = await service.CreateAsync(UserId, ValidCreateRequest() with { Requirements = null });

        Assert.Empty(created.Requirements);
    }

    [Fact]
    public async Task ListAsync_OnlyReturnsApplicationsOwnedByUser_OrderedByDeadline()
    {
        var (service, _, _) = CreateSut();
        await service.CreateAsync(UserId, ValidCreateRequest() with { CollegeName = "Later", Deadline = new DateOnly(2027, 1, 1) });
        await service.CreateAsync(UserId, ValidCreateRequest() with { CollegeName = "Sooner", Deadline = new DateOnly(2026, 10, 15) });
        await service.CreateAsync(OtherUserId, ValidCreateRequest() with { CollegeName = "NotMine" });

        var result = await service.ListAsync(UserId);

        Assert.Equal(2, result.Count);
        Assert.Equal("Sooner", result[0].CollegeName);
        Assert.Equal("Later", result[1].CollegeName);
    }

    [Fact]
    public async Task GetAsync_ForApplicationOwnedByAnotherUser_ThrowsNotFound()
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
            new UpdateCollegeApplicationRequest("New College", ApplicationType.RegularDecision, new DateOnly(2027, 1, 15), CollegeApplicationStatus.Submitted, ["Essay"]));

        Assert.Equal("New College", updated.CollegeName);
        Assert.Equal(ApplicationType.RegularDecision, updated.ApplicationType);
        Assert.Equal(CollegeApplicationStatus.Submitted, updated.Status);
        Assert.Single(updated.Requirements);
        Assert.NotNull(updated.UpdatedAt);
    }

    [Fact]
    public async Task UpdateAsync_ForUnknownApplication_ThrowsNotFound()
    {
        var (service, _, _) = CreateSut();

        await Assert.ThrowsAsync<NotFoundException>(() =>
            service.UpdateAsync(UserId, Guid.NewGuid(), new UpdateCollegeApplicationRequest("X", ApplicationType.Rolling, new DateOnly(2026, 1, 1), CollegeApplicationStatus.NotStarted, null)));
    }

    [Fact]
    public async Task DeleteAsync_RemovesApplication()
    {
        var (service, db, _) = CreateSut();
        var created = await service.CreateAsync(UserId, ValidCreateRequest());

        await service.DeleteAsync(UserId, created.Id);

        Assert.Empty(db.CollegeApplicationsList);
    }

    [Fact]
    public async Task DeleteAsync_ForApplicationOwnedByAnotherUser_ThrowsNotFound_AndDoesNotDelete()
    {
        var (service, db, _) = CreateSut();
        var created = await service.CreateAsync(OtherUserId, ValidCreateRequest());

        await Assert.ThrowsAsync<NotFoundException>(() => service.DeleteAsync(UserId, created.Id));
        Assert.Single(db.CollegeApplicationsList);
    }

    private class FakeAppDbContext : IAppDbContext
    {
        public List<CollegeApplication> CollegeApplicationsList { get; } = new();

        public IQueryable<School> Schools => new List<School>().AsQueryable();
        public IQueryable<User> Users => new List<User>().AsQueryable();
        public IQueryable<Assignment> Assignments => new List<Assignment>().AsQueryable();
        public IQueryable<StudyGroup> StudyGroups => new List<StudyGroup>().AsQueryable();
        public IQueryable<StudyGroupMember> StudyGroupMembers => new List<StudyGroupMember>().AsQueryable();
        public IQueryable<GroupNote> GroupNotes => new List<GroupNote>().AsQueryable();
        public IQueryable<Exam> Exams => new List<Exam>().AsQueryable();
        public IQueryable<Course> Courses => new List<Course>().AsQueryable();
        public IQueryable<SatPlan> SatPlans => new List<SatPlan>().AsQueryable();
        public IQueryable<SatPracticeScore> SatPracticeScores => new List<SatPracticeScore>().AsQueryable();
        public IQueryable<Scholarship> Scholarships => new List<Scholarship>().AsQueryable();
        public IQueryable<CollegeApplication> CollegeApplications => CollegeApplicationsList.AsQueryable();
        public IQueryable<Notification> Notifications => new List<Notification>().AsQueryable();
        public IQueryable<RefreshToken> RefreshTokens => new List<RefreshToken>().AsQueryable();

        public void Add<TEntity>(TEntity entity) where TEntity : class
        {
            if (entity is CollegeApplication application)
            {
                CollegeApplicationsList.Add(application);
                return;
            }

            throw new NotSupportedException($"FakeAppDbContext does not support adding {typeof(TEntity).Name}.");
        }

        public void Remove<TEntity>(TEntity entity) where TEntity : class
        {
            if (entity is CollegeApplication application)
            {
                CollegeApplicationsList.Remove(application);
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
