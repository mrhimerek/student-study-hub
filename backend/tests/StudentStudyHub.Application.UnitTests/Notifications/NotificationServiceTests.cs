using StudentStudyHub.Application.Common.Interfaces;
using StudentStudyHub.Application.Notifications;
using StudentStudyHub.Domain.Entities;
using StudentStudyHub.Domain.Enums;
using StudentStudyHub.Domain.Exceptions;

namespace StudentStudyHub.Application.UnitTests.Notifications;

public class NotificationServiceTests
{
    private static readonly Guid UserId = Guid.NewGuid();
    private static readonly Guid OtherUserId = Guid.NewGuid();

    private static (NotificationService Service, FakeAppDbContext Db) CreateSut()
    {
        var db = new FakeAppDbContext();
        var clock = new FakeDateTimeProvider();
        return (new NotificationService(db, clock), db);
    }

    private static Notification MakeNotification(Guid userId, bool isRead = false) => new()
    {
        Id = Guid.NewGuid(),
        UserId = userId,
        Type = NotificationType.AssignmentDue,
        Title = "Test",
        Body = "Body",
        IsRead = isRead,
    };

    [Fact]
    public async Task ListAsync_ReturnsOnlyOwnNotifications_NewestFirst()
    {
        var (service, db) = CreateSut();
        var older = MakeNotification(UserId);
        older.CreatedAt = DateTime.UtcNow.AddHours(-2);
        var newer = MakeNotification(UserId);
        newer.CreatedAt = DateTime.UtcNow;
        db.NotificationsList.Add(older);
        db.NotificationsList.Add(newer);
        db.NotificationsList.Add(MakeNotification(OtherUserId));

        var result = await service.ListAsync(UserId);

        Assert.Equal(2, result.Count);
        Assert.Equal(newer.Id, result[0].Id);
        Assert.Equal(older.Id, result[1].Id);
    }

    [Fact]
    public async Task ListAsync_UnreadOnly_FiltersReadNotifications()
    {
        var (service, db) = CreateSut();
        db.NotificationsList.Add(MakeNotification(UserId, isRead: true));
        var unread = MakeNotification(UserId, isRead: false);
        db.NotificationsList.Add(unread);

        var result = await service.ListAsync(UserId, unreadOnly: true);

        Assert.Single(result);
        Assert.Equal(unread.Id, result[0].Id);
    }

    [Fact]
    public async Task GetUnreadCountAsync_CountsOnlyOwnUnread()
    {
        var (service, db) = CreateSut();
        db.NotificationsList.Add(MakeNotification(UserId, isRead: false));
        db.NotificationsList.Add(MakeNotification(UserId, isRead: true));
        db.NotificationsList.Add(MakeNotification(OtherUserId, isRead: false));

        var count = await service.GetUnreadCountAsync(UserId);

        Assert.Equal(1, count);
    }

    [Fact]
    public async Task MarkAsReadAsync_SetsIsRead()
    {
        var (service, db) = CreateSut();
        var notification = MakeNotification(UserId);
        db.NotificationsList.Add(notification);

        await service.MarkAsReadAsync(UserId, notification.Id);

        Assert.True(notification.IsRead);
    }

    [Fact]
    public async Task MarkAsReadAsync_ForUnknownOrUnownedNotification_ThrowsNotFound()
    {
        var (service, db) = CreateSut();
        var notification = MakeNotification(OtherUserId);
        db.NotificationsList.Add(notification);

        await Assert.ThrowsAsync<NotFoundException>(() => service.MarkAsReadAsync(UserId, notification.Id));
        await Assert.ThrowsAsync<NotFoundException>(() => service.MarkAsReadAsync(UserId, Guid.NewGuid()));
    }

    [Fact]
    public async Task MarkAllAsReadAsync_MarksOnlyOwnUnreadNotifications()
    {
        var (service, db) = CreateSut();
        var mine1 = MakeNotification(UserId);
        var mine2 = MakeNotification(UserId);
        var theirs = MakeNotification(OtherUserId);
        db.NotificationsList.Add(mine1);
        db.NotificationsList.Add(mine2);
        db.NotificationsList.Add(theirs);

        await service.MarkAllAsReadAsync(UserId);

        Assert.True(mine1.IsRead);
        Assert.True(mine2.IsRead);
        Assert.False(theirs.IsRead);
    }

    private class FakeAppDbContext : IAppDbContext
    {
        public List<Notification> NotificationsList { get; } = new();

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
        public IQueryable<CollegeApplication> CollegeApplications => new List<CollegeApplication>().AsQueryable();
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

        public void Remove<TEntity>(TEntity entity) where TEntity : class
        {
            if (entity is Notification n)
            {
                NotificationsList.Remove(n);
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
