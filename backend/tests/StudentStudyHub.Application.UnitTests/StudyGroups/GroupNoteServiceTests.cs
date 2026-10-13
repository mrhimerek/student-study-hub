using StudentStudyHub.Application.Common.Interfaces;
using StudentStudyHub.Application.StudyGroups;
using StudentStudyHub.Domain.Entities;
using StudentStudyHub.Domain.Enums;
using StudentStudyHub.Domain.Exceptions;

namespace StudentStudyHub.Application.UnitTests.StudyGroups;

public class GroupNoteServiceTests
{
    private static readonly Guid OwnerId = Guid.NewGuid();
    private static readonly Guid MemberId = Guid.NewGuid();
    private static readonly Guid OutsiderId = Guid.NewGuid();
    private static readonly Guid GroupId = Guid.NewGuid();

    private static (GroupNoteService Service, FakeAppDbContext Db, FakeDateTimeProvider Clock) CreateSut()
    {
        var db = new FakeAppDbContext();
        db.StudyGroupsList.Add(new StudyGroup { Id = GroupId, OwnerUserId = OwnerId, Name = "Group" });
        db.StudyGroupMembersList.Add(new StudyGroupMember { StudyGroupId = GroupId, UserId = OwnerId, Role = GroupMemberRole.Owner });
        db.StudyGroupMembersList.Add(new StudyGroupMember { StudyGroupId = GroupId, UserId = MemberId, Role = GroupMemberRole.Member });
        db.UsersList.Add(new User { Id = OwnerId, FirstName = "Olivia", LastName = "Owner" });
        db.UsersList.Add(new User { Id = MemberId, FirstName = "Mia", LastName = "Member" });

        var clock = new FakeDateTimeProvider();
        var service = new GroupNoteService(db, clock);
        return (service, db, clock);
    }

    [Fact]
    public async Task ListAsync_ForNonMember_ThrowsNotFound()
    {
        var (service, _, _) = CreateSut();

        await Assert.ThrowsAsync<NotFoundException>(() => service.ListAsync(OutsiderId, GroupId));
    }

    [Fact]
    public async Task CreateAsync_PersistsNote_WithAuthorInfo()
    {
        var (service, db, _) = CreateSut();

        var created = await service.CreateAsync(MemberId, GroupId, new CreateGroupNoteRequest("Chapter 3 summary", "Key points..."));

        Assert.Single(db.GroupNotesList);
        Assert.Equal("Mia Member", created.AuthorName);
        Assert.True(created.IsMine);
    }

    [Fact]
    public async Task CreateAsync_ForNonMember_ThrowsNotFound()
    {
        var (service, _, _) = CreateSut();

        await Assert.ThrowsAsync<NotFoundException>(() =>
            service.CreateAsync(OutsiderId, GroupId, new CreateGroupNoteRequest("Title", null)));
    }

    [Fact]
    public async Task UpdateAsync_ByAuthor_UpdatesFields()
    {
        var (service, _, clock) = CreateSut();
        var created = await service.CreateAsync(MemberId, GroupId, new CreateGroupNoteRequest("Original", null));
        clock.UtcNow = clock.UtcNow.AddHours(1);

        var updated = await service.UpdateAsync(MemberId, GroupId, created.Id, new UpdateGroupNoteRequest("Updated title", "New content"));

        Assert.Equal("Updated title", updated.Title);
        Assert.Equal("New content", updated.Content);
        Assert.NotNull(updated.UpdatedAt);
    }

    [Fact]
    public async Task UpdateAsync_ByNonAuthorMember_ThrowsForbidden()
    {
        var (service, _, _) = CreateSut();
        var created = await service.CreateAsync(MemberId, GroupId, new CreateGroupNoteRequest("Original", null));

        await Assert.ThrowsAsync<ForbiddenException>(() =>
            service.UpdateAsync(OwnerId, GroupId, created.Id, new UpdateGroupNoteRequest("Hijacked", null)));
    }

    [Fact]
    public async Task DeleteAsync_ByAuthor_RemovesNote()
    {
        var (service, db, _) = CreateSut();
        var created = await service.CreateAsync(MemberId, GroupId, new CreateGroupNoteRequest("Original", null));

        await service.DeleteAsync(MemberId, GroupId, created.Id);

        Assert.Empty(db.GroupNotesList);
    }

    [Fact]
    public async Task DeleteAsync_ByGroupOwner_RemovesOthersNote()
    {
        var (service, db, _) = CreateSut();
        var created = await service.CreateAsync(MemberId, GroupId, new CreateGroupNoteRequest("Original", null));

        await service.DeleteAsync(OwnerId, GroupId, created.Id);

        Assert.Empty(db.GroupNotesList);
    }

    [Fact]
    public async Task DeleteAsync_ByUnrelatedMember_ThrowsForbidden()
    {
        var (service, db, _) = CreateSut();
        var created = await service.CreateAsync(OwnerId, GroupId, new CreateGroupNoteRequest("Owner note", null));
        db.StudyGroupMembersList.Add(new StudyGroupMember { StudyGroupId = GroupId, UserId = Guid.NewGuid(), Role = GroupMemberRole.Member });
        var otherMemberId = db.StudyGroupMembersList[^1].UserId;

        await Assert.ThrowsAsync<ForbiddenException>(() => service.DeleteAsync(otherMemberId, GroupId, created.Id));
    }

    private class FakeAppDbContext : IAppDbContext
    {
        public List<StudyGroup> StudyGroupsList { get; } = new();
        public List<StudyGroupMember> StudyGroupMembersList { get; } = new();
        public List<GroupNote> GroupNotesList { get; } = new();
        public List<User> UsersList { get; } = new();

        public IQueryable<School> Schools => new List<School>().AsQueryable();
        public IQueryable<User> Users => UsersList.AsQueryable();
        public IQueryable<Assignment> Assignments => new List<Assignment>().AsQueryable();
        public IQueryable<StudyGroup> StudyGroups => StudyGroupsList.AsQueryable();
        public IQueryable<StudyGroupMember> StudyGroupMembers => StudyGroupMembersList.AsQueryable();
        public IQueryable<GroupNote> GroupNotes => GroupNotesList.AsQueryable();
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
                case GroupNote note:
                    GroupNotesList.Add(note);
                    return;
                case StudyGroupMember member:
                    StudyGroupMembersList.Add(member);
                    return;
                default:
                    throw new NotSupportedException($"FakeAppDbContext does not support adding {typeof(TEntity).Name}.");
            }
        }

        public void Remove<TEntity>(TEntity entity) where TEntity : class
        {
            if (entity is GroupNote note)
            {
                GroupNotesList.Remove(note);
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
