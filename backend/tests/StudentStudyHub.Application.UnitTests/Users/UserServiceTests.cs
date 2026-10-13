using StudentStudyHub.Application.Common.Interfaces;
using StudentStudyHub.Application.Users;
using StudentStudyHub.Domain.Entities;
using StudentStudyHub.Domain.Exceptions;

namespace StudentStudyHub.Application.UnitTests.Users;

public class UserServiceTests
{
    private static (UserService Service, FakeAppDbContext Db, FakeDateTimeProvider Clock, User User) CreateSut()
    {
        var db = new FakeAppDbContext();
        var clock = new FakeDateTimeProvider();
        var user = new User
        {
            Email = "priya@example.com",
            PasswordHash = "hash",
            FirstName = "Priya",
            LastName = "Sharma",
            GradeLevel = 10,
        };
        db.UsersList.Add(user);

        var service = new UserService(db, clock);
        return (service, db, clock, user);
    }

    [Fact]
    public async Task GetProfileAsync_ReturnsCurrentUser_WithNoSchool()
    {
        var (service, _, _, user) = CreateSut();

        var profile = await service.GetProfileAsync(user.Id);

        Assert.Equal(user.Id, profile.Id);
        Assert.Equal("priya@example.com", profile.Email);
        Assert.Equal("Priya", profile.FirstName);
        Assert.Equal("Sharma", profile.LastName);
        Assert.Equal(10, profile.GradeLevel);
        Assert.Null(profile.SchoolName);
    }

    [Fact]
    public async Task GetProfileAsync_ResolvesSchoolName_WhenUserHasSchool()
    {
        var (service, db, _, user) = CreateSut();
        var school = new School { Name = "Lincoln High" };
        db.SchoolsList.Add(school);
        user.SchoolId = school.Id;

        var profile = await service.GetProfileAsync(user.Id);

        Assert.Equal("Lincoln High", profile.SchoolName);
    }

    [Fact]
    public async Task GetProfileAsync_ForUnknownUser_ThrowsNotFound()
    {
        var (service, _, _, _) = CreateSut();

        await Assert.ThrowsAsync<NotFoundException>(() => service.GetProfileAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task UpdateProfileAsync_UpdatesNameAndGrade_AndSetsUpdatedAt()
    {
        var (service, _, clock, user) = CreateSut();
        clock.UtcNow = clock.UtcNow.AddHours(1);

        var updated = await service.UpdateProfileAsync(
            user.Id,
            new UpdateProfileRequest("Priyanka", "Sharma", 11, null));

        Assert.Equal("Priyanka", updated.FirstName);
        Assert.Equal(11, updated.GradeLevel);
        Assert.Equal(clock.UtcNow, user.UpdatedAt);
    }

    [Fact]
    public async Task UpdateProfileAsync_WithNewSchoolName_CreatesSchool_AndAssignsIt()
    {
        var (service, db, _, user) = CreateSut();

        var updated = await service.UpdateProfileAsync(
            user.Id,
            new UpdateProfileRequest("Priya", "Sharma", 10, "Lincoln High"));

        Assert.Equal("Lincoln High", updated.SchoolName);
        Assert.Single(db.SchoolsList);
        Assert.Equal(db.SchoolsList[0].Id, user.SchoolId);
    }

    [Fact]
    public async Task UpdateProfileAsync_WithExistingSchoolName_ReusesSchool_DoesNotDuplicate()
    {
        var (service, db, _, user) = CreateSut();
        var existing = new School { Name = "Lincoln High" };
        db.SchoolsList.Add(existing);

        await service.UpdateProfileAsync(user.Id, new UpdateProfileRequest("Priya", "Sharma", 10, "Lincoln High"));

        Assert.Single(db.SchoolsList);
        Assert.Equal(existing.Id, user.SchoolId);
    }

    [Fact]
    public async Task UpdateProfileAsync_WithNullSchoolName_ClearsSchool()
    {
        var (service, db, _, user) = CreateSut();
        var school = new School { Name = "Lincoln High" };
        db.SchoolsList.Add(school);
        user.SchoolId = school.Id;

        var updated = await service.UpdateProfileAsync(user.Id, new UpdateProfileRequest("Priya", "Sharma", 10, null));

        Assert.Null(updated.SchoolName);
        Assert.Null(user.SchoolId);
    }

    [Fact]
    public async Task UpdateProfileAsync_ForUnknownUser_ThrowsNotFound()
    {
        var (service, _, _, _) = CreateSut();

        await Assert.ThrowsAsync<NotFoundException>(() =>
            service.UpdateProfileAsync(Guid.NewGuid(), new UpdateProfileRequest("X", "Y", 9, null)));
    }

    private class FakeAppDbContext : IAppDbContext
    {
        public List<User> UsersList { get; } = new();
        public List<School> SchoolsList { get; } = new();

        public IQueryable<School> Schools => SchoolsList.AsQueryable();
        public IQueryable<User> Users => UsersList.AsQueryable();
        public IQueryable<Assignment> Assignments => new List<Assignment>().AsQueryable();
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
            switch (entity)
            {
                case School school:
                    SchoolsList.Add(school);
                    return;
                case User user:
                    UsersList.Add(user);
                    return;
                default:
                    throw new NotSupportedException($"FakeAppDbContext does not support adding {typeof(TEntity).Name}.");
            }
        }

        public void Remove<TEntity>(TEntity entity) where TEntity : class
        {
            throw new NotSupportedException($"FakeAppDbContext does not support removing {typeof(TEntity).Name}.");
        }

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => Task.FromResult(0);
    }

    private class FakeDateTimeProvider : IDateTimeProvider
    {
        public DateTime UtcNow { get; set; } = DateTime.UtcNow;
    }
}
