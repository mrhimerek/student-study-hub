using StudentStudyHub.Application.Calendar;
using StudentStudyHub.Application.Common.Interfaces;
using StudentStudyHub.Domain.Entities;
using StudentStudyHub.Domain.Enums;

namespace StudentStudyHub.Application.UnitTests.Calendar;

public class CalendarServiceTests
{
    private static readonly Guid UserId = Guid.NewGuid();
    private static readonly Guid OtherUserId = Guid.NewGuid();

    [Fact]
    public async Task GetEventsAsync_AggregatesAllEntityTypes_WithinRange()
    {
        var db = new FakeAppDbContext();
        db.AssignmentsList.Add(new Assignment
        {
            Id = Guid.NewGuid(), UserId = UserId, Subject = "Math", Title = "Problem Set 3",
            DueDate = new DateTime(2026, 5, 10),
        });
        db.ExamsList.Add(new Exam
        {
            Id = Guid.NewGuid(), UserId = UserId, Subject = "Physics", Title = "Midterm",
            ExamDate = new DateTime(2026, 5, 12),
        });
        db.ScholarshipsList.Add(new Scholarship
        {
            Id = Guid.NewGuid(), UserId = UserId, Name = "Merit Award",
            Deadline = new DateOnly(2026, 5, 15), Status = ScholarshipStatus.Researching,
        });
        db.CollegeApplicationsList.Add(new CollegeApplication
        {
            Id = Guid.NewGuid(), UserId = UserId, CollegeName = "State University",
            Deadline = new DateOnly(2026, 5, 20), ApplicationType = ApplicationType.EarlyDecision,
        });
        db.SatPlansList.Add(new SatPlan
        {
            Id = Guid.NewGuid(), UserId = UserId, TargetTestDate = new DateOnly(2026, 5, 25), GoalScore = 1450,
        });

        var service = new CalendarService(db);

        var events = await service.GetEventsAsync(UserId, new DateOnly(2026, 5, 1), new DateOnly(2026, 5, 31));

        Assert.Equal(5, events.Count);
        Assert.Equal(CalendarEventType.Assignment, events[0].Type);
        Assert.Equal(CalendarEventType.Exam, events[1].Type);
        Assert.Equal(CalendarEventType.Scholarship, events[2].Type);
        Assert.Equal(CalendarEventType.CollegeApplication, events[3].Type);
        Assert.Equal(CalendarEventType.SatTarget, events[4].Type);
        Assert.True(events.Zip(events.Skip(1), (a, b) => a.Date <= b.Date).All(x => x));
    }

    [Fact]
    public async Task GetEventsAsync_FiltersOutOfRangeEvents()
    {
        var db = new FakeAppDbContext();
        db.AssignmentsList.Add(new Assignment
        {
            Id = Guid.NewGuid(), UserId = UserId, Subject = "Math", Title = "In range",
            DueDate = new DateTime(2026, 5, 10),
        });
        db.AssignmentsList.Add(new Assignment
        {
            Id = Guid.NewGuid(), UserId = UserId, Subject = "Math", Title = "Out of range",
            DueDate = new DateTime(2026, 6, 10),
        });

        var service = new CalendarService(db);

        var events = await service.GetEventsAsync(UserId, new DateOnly(2026, 5, 1), new DateOnly(2026, 5, 31));

        Assert.Single(events);
        Assert.Equal("In range", events[0].Title);
    }

    [Fact]
    public async Task GetEventsAsync_ScopesToOwningUser()
    {
        var db = new FakeAppDbContext();
        db.ScholarshipsList.Add(new Scholarship
        {
            Id = Guid.NewGuid(), UserId = OtherUserId, Name = "Not mine",
            Deadline = new DateOnly(2026, 5, 15),
        });

        var service = new CalendarService(db);

        var events = await service.GetEventsAsync(UserId, new DateOnly(2026, 5, 1), new DateOnly(2026, 5, 31));

        Assert.Empty(events);
    }

    [Fact]
    public async Task GetEventsAsync_WhenNoEventsExist_ReturnsEmptyList()
    {
        var db = new FakeAppDbContext();
        var service = new CalendarService(db);

        var events = await service.GetEventsAsync(UserId, new DateOnly(2026, 5, 1), new DateOnly(2026, 5, 31));

        Assert.Empty(events);
    }

    [Fact]
    public async Task GetEventsAsync_OrdersEventsByDate()
    {
        var db = new FakeAppDbContext();
        db.ExamsList.Add(new Exam
        {
            Id = Guid.NewGuid(), UserId = UserId, Subject = "Chem", Title = "Later exam",
            ExamDate = new DateTime(2026, 5, 20),
        });
        db.AssignmentsList.Add(new Assignment
        {
            Id = Guid.NewGuid(), UserId = UserId, Subject = "Math", Title = "Earlier assignment",
            DueDate = new DateTime(2026, 5, 5),
        });

        var service = new CalendarService(db);

        var events = await service.GetEventsAsync(UserId, new DateOnly(2026, 5, 1), new DateOnly(2026, 5, 31));

        Assert.Equal(2, events.Count);
        Assert.Equal("Earlier assignment", events[0].Title);
        Assert.Equal("Later exam", events[1].Title);
    }

    private class FakeAppDbContext : IAppDbContext
    {
        public List<Assignment> AssignmentsList { get; } = new();
        public List<Exam> ExamsList { get; } = new();
        public List<Scholarship> ScholarshipsList { get; } = new();
        public List<CollegeApplication> CollegeApplicationsList { get; } = new();
        public List<SatPlan> SatPlansList { get; } = new();

        public IQueryable<School> Schools => new List<School>().AsQueryable();
        public IQueryable<User> Users => new List<User>().AsQueryable();
        public IQueryable<Assignment> Assignments => AssignmentsList.AsQueryable();
        public IQueryable<StudyGroup> StudyGroups => new List<StudyGroup>().AsQueryable();
        public IQueryable<StudyGroupMember> StudyGroupMembers => new List<StudyGroupMember>().AsQueryable();
        public IQueryable<GroupNote> GroupNotes => new List<GroupNote>().AsQueryable();
        public IQueryable<Exam> Exams => ExamsList.AsQueryable();
        public IQueryable<Course> Courses => new List<Course>().AsQueryable();
        public IQueryable<SatPlan> SatPlans => SatPlansList.AsQueryable();
        public IQueryable<SatPracticeScore> SatPracticeScores => new List<SatPracticeScore>().AsQueryable();
        public IQueryable<Scholarship> Scholarships => ScholarshipsList.AsQueryable();
        public IQueryable<CollegeApplication> CollegeApplications => CollegeApplicationsList.AsQueryable();
        public IQueryable<Notification> Notifications => new List<Notification>().AsQueryable();
        public IQueryable<RefreshToken> RefreshTokens => new List<RefreshToken>().AsQueryable();

        public void Add<TEntity>(TEntity entity) where TEntity : class =>
            throw new NotSupportedException("CalendarService is read-only and should never call Add.");

        public void Remove<TEntity>(TEntity entity) where TEntity : class =>
            throw new NotSupportedException("CalendarService is read-only and should never call Remove.");

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => Task.FromResult(0);
    }
}
