using Microsoft.Extensions.Logging.Abstractions;
using StudentStudyHub.Application.Common.Interfaces;
using StudentStudyHub.Application.Notifications;
using StudentStudyHub.Domain.Entities;
using StudentStudyHub.Domain.Enums;

namespace StudentStudyHub.Application.UnitTests.Notifications;

public class DeadlineReminderServiceTests
{
    private static readonly Guid UserId = Guid.NewGuid();

    private static (DeadlineReminderService Service, FakeAppDbContext Db, FakeEmailService Email) CreateSut(DateTime utcNow)
    {
        var db = new FakeAppDbContext();
        db.UsersList.Add(new User { Id = UserId, Email = "student@example.com", FirstName = "Stu", LastName = "Dent" });
        var clock = new FakeDateTimeProvider { UtcNow = utcNow };
        var email = new FakeEmailService();
        var service = new DeadlineReminderService(db, email, clock, NullLogger<DeadlineReminderService>.Instance);
        return (service, db, email);
    }

    [Fact]
    public async Task ScanAndNotifyAsync_CreatesNotification_ForAssignmentDueInOneDay()
    {
        var now = new DateTime(2026, 5, 1, 12, 0, 0, DateTimeKind.Utc);
        var (service, db, email) = CreateSut(now);
        db.AssignmentsList.Add(new Assignment
        {
            Id = Guid.NewGuid(), UserId = UserId, Subject = "Math", Title = "HW",
            DueDate = new DateTime(2026, 5, 2), Status = AssignmentStatus.NotStarted,
        });

        var created = await service.ScanAndNotifyAsync();

        Assert.Equal(1, created);
        Assert.Single(db.NotificationsList);
        Assert.Equal(NotificationType.AssignmentDue, db.NotificationsList[0].Type);
        Assert.True(db.NotificationsList[0].SentViaEmail);
        Assert.Single(email.SentTo);
    }

    [Fact]
    public async Task ScanAndNotifyAsync_CreatesNotification_ForItemsDueInThreeDays()
    {
        var now = new DateTime(2026, 5, 1, 12, 0, 0, DateTimeKind.Utc);
        var (service, db, _) = CreateSut(now);
        db.ExamsList.Add(new Exam
        {
            Id = Guid.NewGuid(), UserId = UserId, Subject = "Chem", Title = "Final",
            ExamDate = new DateTime(2026, 5, 4),
        });

        var created = await service.ScanAndNotifyAsync();

        Assert.Equal(1, created);
        Assert.Equal(NotificationType.ExamSoon, db.NotificationsList[0].Type);
    }

    [Fact]
    public async Task ScanAndNotifyAsync_IgnoresItems_OutsideReminderWindow()
    {
        var now = new DateTime(2026, 5, 1, 12, 0, 0, DateTimeKind.Utc);
        var (service, db, _) = CreateSut(now);
        db.AssignmentsList.Add(new Assignment
        {
            Id = Guid.NewGuid(), UserId = UserId, Subject = "Math", Title = "Far HW",
            DueDate = new DateTime(2026, 5, 10), Status = AssignmentStatus.NotStarted,
        });

        var created = await service.ScanAndNotifyAsync();

        Assert.Equal(0, created);
        Assert.Empty(db.NotificationsList);
    }

    [Fact]
    public async Task ScanAndNotifyAsync_IgnoresCompletedAssignments()
    {
        var now = new DateTime(2026, 5, 1, 12, 0, 0, DateTimeKind.Utc);
        var (service, db, _) = CreateSut(now);
        db.AssignmentsList.Add(new Assignment
        {
            Id = Guid.NewGuid(), UserId = UserId, Subject = "Math", Title = "Done HW",
            DueDate = new DateTime(2026, 5, 2), Status = AssignmentStatus.Done,
        });

        var created = await service.ScanAndNotifyAsync();

        Assert.Equal(0, created);
    }

    [Fact]
    public async Task ScanAndNotifyAsync_IgnoresAwardedScholarshipsAndAcceptedApplications()
    {
        var now = new DateTime(2026, 5, 1, 12, 0, 0, DateTimeKind.Utc);
        var (service, db, _) = CreateSut(now);
        db.ScholarshipsList.Add(new Scholarship
        {
            Id = Guid.NewGuid(), UserId = UserId, Name = "Awarded", Deadline = new DateOnly(2026, 5, 2),
            Status = ScholarshipStatus.Awarded,
        });
        db.CollegeApplicationsList.Add(new CollegeApplication
        {
            Id = Guid.NewGuid(), UserId = UserId, CollegeName = "Accepted U", Deadline = new DateOnly(2026, 5, 4),
            Status = CollegeApplicationStatus.Accepted,
        });

        var created = await service.ScanAndNotifyAsync();

        Assert.Equal(0, created);
    }

    [Fact]
    public async Task ScanAndNotifyAsync_DoesNotDuplicateNotification_WhenAlreadyNotifiedToday()
    {
        var now = new DateTime(2026, 5, 1, 9, 0, 0, DateTimeKind.Utc);
        var (service, db, _) = CreateSut(now);
        var assignmentId = Guid.NewGuid();
        db.AssignmentsList.Add(new Assignment
        {
            Id = assignmentId, UserId = UserId, Subject = "Math", Title = "HW",
            DueDate = new DateTime(2026, 5, 2), Status = AssignmentStatus.NotStarted,
        });

        var firstRun = await service.ScanAndNotifyAsync();
        var secondRun = await service.ScanAndNotifyAsync();

        Assert.Equal(1, firstRun);
        Assert.Equal(0, secondRun);
        Assert.Single(db.NotificationsList);
    }

    private class FakeEmailService : IEmailService
    {
        public List<string> SentTo { get; } = new();

        public Task SendEmailAsync(string toEmail, string subject, string htmlBody, CancellationToken cancellationToken = default)
        {
            SentTo.Add(toEmail);
            return Task.CompletedTask;
        }
    }

    private class FakeAppDbContext : IAppDbContext
    {
        public List<User> UsersList { get; } = new();
        public List<Assignment> AssignmentsList { get; } = new();
        public List<Exam> ExamsList { get; } = new();
        public List<Scholarship> ScholarshipsList { get; } = new();
        public List<CollegeApplication> CollegeApplicationsList { get; } = new();
        public List<Notification> NotificationsList { get; } = new();

        public IQueryable<School> Schools => new List<School>().AsQueryable();
        public IQueryable<User> Users => UsersList.AsQueryable();
        public IQueryable<Assignment> Assignments => AssignmentsList.AsQueryable();
        public IQueryable<StudyGroup> StudyGroups => new List<StudyGroup>().AsQueryable();
        public IQueryable<StudyGroupMember> StudyGroupMembers => new List<StudyGroupMember>().AsQueryable();
        public IQueryable<GroupNote> GroupNotes => new List<GroupNote>().AsQueryable();
        public IQueryable<Exam> Exams => ExamsList.AsQueryable();
        public IQueryable<Course> Courses => new List<Course>().AsQueryable();
        public IQueryable<SatPlan> SatPlans => new List<SatPlan>().AsQueryable();
        public IQueryable<SatPracticeScore> SatPracticeScores => new List<SatPracticeScore>().AsQueryable();
        public IQueryable<Scholarship> Scholarships => ScholarshipsList.AsQueryable();
        public IQueryable<CollegeApplication> CollegeApplications => CollegeApplicationsList.AsQueryable();
        public IQueryable<Notification> Notifications => NotificationsList.AsQueryable();
        public IQueryable<RefreshToken> RefreshTokens => new List<RefreshToken>().AsQueryable();

        public void Add<TEntity>(TEntity entity) where TEntity : class
        {
            if (entity is Notification n)
            {
                NotificationsList.Add(n);
                return;
            }

            throw new NotSupportedException($"FakeAppDbContext does not support adding {typeof(TEntity).Name}.");
        }

        public void Remove<TEntity>(TEntity entity) where TEntity : class =>
            throw new NotSupportedException($"FakeAppDbContext does not support removing {typeof(TEntity).Name}.");

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => Task.FromResult(0);
    }

    private class FakeDateTimeProvider : IDateTimeProvider
    {
        public DateTime UtcNow { get; set; } = DateTime.UtcNow;
    }
}
