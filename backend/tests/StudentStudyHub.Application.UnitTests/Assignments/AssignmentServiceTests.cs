using StudentStudyHub.Application.Assignments;
using StudentStudyHub.Application.Common.Interfaces;
using StudentStudyHub.Domain.Entities;
using StudentStudyHub.Domain.Enums;
using StudentStudyHub.Domain.Exceptions;

namespace StudentStudyHub.Application.UnitTests.Assignments;

public class AssignmentServiceTests
{
    private static readonly Guid UserId = Guid.NewGuid();
    private static readonly Guid OtherUserId = Guid.NewGuid();

    private static (AssignmentService Service, FakeAppDbContext Db, FakeDateTimeProvider Clock) CreateSut()
    {
        var db = new FakeAppDbContext();
        var clock = new FakeDateTimeProvider();
        var service = new AssignmentService(db, clock);
        return (service, db, clock);
    }

    private static CreateAssignmentRequest ValidCreateRequest() =>
        new("Math", "Algebra worksheet", "Chapter 4, problems 1-20", DateTime.UtcNow.AddDays(3), Priority.High);

    [Fact]
    public async Task CreateAsync_PersistsAssignment_WithNotStartedStatus()
    {
        var (service, db, _) = CreateSut();

        var created = await service.CreateAsync(UserId, ValidCreateRequest());

        Assert.Single(db.AssignmentsList);
        Assert.Equal(AssignmentStatus.NotStarted, created.Status);
        Assert.Equal("Math", created.Subject);
        Assert.Equal(UserId, db.AssignmentsList[0].UserId);
    }

    [Fact]
    public async Task ListAsync_OnlyReturnsAssignmentsOwnedByUser_OrderedByDueDate()
    {
        var (service, _, _) = CreateSut();
        await service.CreateAsync(UserId, ValidCreateRequest() with { Title = "Later", DueDate = DateTime.UtcNow.AddDays(5) });
        await service.CreateAsync(UserId, ValidCreateRequest() with { Title = "Sooner", DueDate = DateTime.UtcNow.AddDays(1) });
        await service.CreateAsync(OtherUserId, ValidCreateRequest() with { Title = "NotMine" });

        var result = await service.ListAsync(UserId, status: null);

        Assert.Equal(2, result.Count);
        Assert.Equal("Sooner", result[0].Title);
        Assert.Equal("Later", result[1].Title);
    }

    [Fact]
    public async Task ListAsync_FiltersByStatus()
    {
        var (service, _, _) = CreateSut();
        var done = await service.CreateAsync(UserId, ValidCreateRequest() with { Title = "Done one" });
        await service.UpdateAsync(UserId, done.Id, new UpdateAssignmentRequest("Math", "Done one", null, done.DueDate, Priority.Low, AssignmentStatus.Done));
        await service.CreateAsync(UserId, ValidCreateRequest() with { Title = "Still pending" });

        var result = await service.ListAsync(UserId, AssignmentStatus.Done);

        Assert.Single(result);
        Assert.Equal("Done one", result[0].Title);
    }

    [Fact]
    public async Task GetAsync_ForAssignmentOwnedByAnotherUser_ThrowsNotFound()
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
            new UpdateAssignmentRequest("Science", "Lab report", "Updated notes", created.DueDate.AddDays(1), Priority.Medium, AssignmentStatus.InProgress));

        Assert.Equal("Science", updated.Subject);
        Assert.Equal(AssignmentStatus.InProgress, updated.Status);
        Assert.NotNull(updated.UpdatedAt);
    }

    [Fact]
    public async Task UpdateAsync_ForUnknownAssignment_ThrowsNotFound()
    {
        var (service, _, _) = CreateSut();

        await Assert.ThrowsAsync<NotFoundException>(() =>
            service.UpdateAsync(UserId, Guid.NewGuid(), new UpdateAssignmentRequest("Math", "X", null, DateTime.UtcNow, Priority.Low, AssignmentStatus.Done)));
    }

    [Fact]
    public async Task DeleteAsync_RemovesAssignment()
    {
        var (service, db, _) = CreateSut();
        var created = await service.CreateAsync(UserId, ValidCreateRequest());

        await service.DeleteAsync(UserId, created.Id);

        Assert.Empty(db.AssignmentsList);
    }

    [Fact]
    public async Task DeleteAsync_ForAssignmentOwnedByAnotherUser_ThrowsNotFound_AndDoesNotDelete()
    {
        var (service, db, _) = CreateSut();
        var created = await service.CreateAsync(OtherUserId, ValidCreateRequest());

        await Assert.ThrowsAsync<NotFoundException>(() => service.DeleteAsync(UserId, created.Id));
        Assert.Single(db.AssignmentsList);
    }

    private class FakeAppDbContext : IAppDbContext
    {
        public List<Assignment> AssignmentsList { get; } = new();

        public IQueryable<School> Schools => new List<School>().AsQueryable();
        public IQueryable<User> Users => new List<User>().AsQueryable();
        public IQueryable<Assignment> Assignments => AssignmentsList.AsQueryable();
        public IQueryable<StudyGroup> StudyGroups => new List<StudyGroup>().AsQueryable();
        public IQueryable<StudyGroupMember> StudyGroupMembers => new List<StudyGroupMember>().AsQueryable();
        public IQueryable<GroupNote> GroupNotes => new List<GroupNote>().AsQueryable();
        public IQueryable<Exam> Exams => new List<Exam>().AsQueryable();
        public IQueryable<Course> Courses => new List<Course>().AsQueryable();
        public IQueryable<SatPlan> SatPlans => new List<SatPlan>().AsQueryable();
        public IQueryable<SatPracticeScore> SatPracticeScores => new List<SatPracticeScore>().AsQueryable();
        public IQueryable<Scholarship> Scholarships => new List<Scholarship>().AsQueryable();
        public IQueryable<CollegeApplication> CollegeApplications => new List<CollegeApplication>().AsQueryable();
        public IQueryable<Notification> Notifications => new List<Notification>().AsQueryable();
        public IQueryable<RefreshToken> RefreshTokens => new List<RefreshToken>().AsQueryable();

        public void Add<TEntity>(TEntity entity) where TEntity : class
        {
            if (entity is Assignment assignment)
            {
                AssignmentsList.Add(assignment);
                return;
            }

            throw new NotSupportedException($"FakeAppDbContext does not support adding {typeof(TEntity).Name}.");
        }

        public void Remove<TEntity>(TEntity entity) where TEntity : class
        {
            if (entity is Assignment assignment)
            {
                AssignmentsList.Remove(assignment);
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
