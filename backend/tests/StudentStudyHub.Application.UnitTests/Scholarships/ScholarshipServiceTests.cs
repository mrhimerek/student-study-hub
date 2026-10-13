using StudentStudyHub.Application.Common.Interfaces;
using StudentStudyHub.Application.Scholarships;
using StudentStudyHub.Domain.Entities;
using StudentStudyHub.Domain.Enums;
using StudentStudyHub.Domain.Exceptions;

namespace StudentStudyHub.Application.UnitTests.Scholarships;

public class ScholarshipServiceTests
{
    private static readonly Guid UserId = Guid.NewGuid();
    private static readonly Guid OtherUserId = Guid.NewGuid();

    private static (ScholarshipService Service, FakeAppDbContext Db, FakeDateTimeProvider Clock) CreateSut()
    {
        var db = new FakeAppDbContext();
        var clock = new FakeDateTimeProvider();
        var service = new ScholarshipService(db, clock);
        return (service, db, clock);
    }

    private static CreateScholarshipRequest ValidCreateRequest() =>
        new("Merit Scholarship", 5000m, new DateOnly(2026, 3, 1), ScholarshipStatus.Researching, ["Essay", "2 Recommendation Letters"]);

    [Fact]
    public async Task CreateAsync_PersistsScholarship_WithRequirements()
    {
        var (service, db, _) = CreateSut();

        var created = await service.CreateAsync(UserId, ValidCreateRequest());

        Assert.Single(db.ScholarshipsList);
        Assert.Equal("Merit Scholarship", created.Name);
        Assert.Equal(UserId, db.ScholarshipsList[0].UserId);
        Assert.Equal(2, created.Requirements.Count);
        Assert.Contains("Essay", created.Requirements);
    }

    [Fact]
    public async Task CreateAsync_WithNoRequirements_ReturnsEmptyList()
    {
        var (service, _, _) = CreateSut();

        var created = await service.CreateAsync(UserId, ValidCreateRequest() with { Requirements = null });

        Assert.Empty(created.Requirements);
    }

    [Fact]
    public async Task ListAsync_OnlyReturnsScholarshipsOwnedByUser_OrderedByDeadline()
    {
        var (service, _, _) = CreateSut();
        await service.CreateAsync(UserId, ValidCreateRequest() with { Name = "Later", Deadline = new DateOnly(2026, 6, 1) });
        await service.CreateAsync(UserId, ValidCreateRequest() with { Name = "Sooner", Deadline = new DateOnly(2026, 1, 1) });
        await service.CreateAsync(OtherUserId, ValidCreateRequest() with { Name = "NotMine" });

        var result = await service.ListAsync(UserId);

        Assert.Equal(2, result.Count);
        Assert.Equal("Sooner", result[0].Name);
        Assert.Equal("Later", result[1].Name);
    }

    [Fact]
    public async Task GetAsync_ForScholarshipOwnedByAnotherUser_ThrowsNotFound()
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
            new UpdateScholarshipRequest("Updated Name", 7500m, new DateOnly(2026, 4, 1), ScholarshipStatus.Applying, ["Essay"]));

        Assert.Equal("Updated Name", updated.Name);
        Assert.Equal(7500m, updated.Amount);
        Assert.Equal(ScholarshipStatus.Applying, updated.Status);
        Assert.Single(updated.Requirements);
        Assert.NotNull(updated.UpdatedAt);
    }

    [Fact]
    public async Task UpdateAsync_ForUnknownScholarship_ThrowsNotFound()
    {
        var (service, _, _) = CreateSut();

        await Assert.ThrowsAsync<NotFoundException>(() =>
            service.UpdateAsync(UserId, Guid.NewGuid(), new UpdateScholarshipRequest("X", null, new DateOnly(2026, 1, 1), ScholarshipStatus.Researching, null)));
    }

    [Fact]
    public async Task DeleteAsync_RemovesScholarship()
    {
        var (service, db, _) = CreateSut();
        var created = await service.CreateAsync(UserId, ValidCreateRequest());

        await service.DeleteAsync(UserId, created.Id);

        Assert.Empty(db.ScholarshipsList);
    }

    [Fact]
    public async Task DeleteAsync_ForScholarshipOwnedByAnotherUser_ThrowsNotFound_AndDoesNotDelete()
    {
        var (service, db, _) = CreateSut();
        var created = await service.CreateAsync(OtherUserId, ValidCreateRequest());

        await Assert.ThrowsAsync<NotFoundException>(() => service.DeleteAsync(UserId, created.Id));
        Assert.Single(db.ScholarshipsList);
    }

    private class FakeAppDbContext : IAppDbContext
    {
        public List<Scholarship> ScholarshipsList { get; } = new();

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
        public IQueryable<Scholarship> Scholarships => ScholarshipsList.AsQueryable();
        public IQueryable<CollegeApplication> CollegeApplications => new List<CollegeApplication>().AsQueryable();
        public IQueryable<Notification> Notifications => new List<Notification>().AsQueryable();
        public IQueryable<RefreshToken> RefreshTokens => new List<RefreshToken>().AsQueryable();

        public void Add<TEntity>(TEntity entity) where TEntity : class
        {
            if (entity is Scholarship scholarship)
            {
                ScholarshipsList.Add(scholarship);
                return;
            }

            throw new NotSupportedException($"FakeAppDbContext does not support adding {typeof(TEntity).Name}.");
        }

        public void Remove<TEntity>(TEntity entity) where TEntity : class
        {
            if (entity is Scholarship scholarship)
            {
                ScholarshipsList.Remove(scholarship);
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
