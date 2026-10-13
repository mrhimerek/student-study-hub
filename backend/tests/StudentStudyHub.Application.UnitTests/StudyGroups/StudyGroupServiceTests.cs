using StudentStudyHub.Application.Common.Interfaces;
using StudentStudyHub.Application.StudyGroups;
using StudentStudyHub.Domain.Entities;
using StudentStudyHub.Domain.Enums;
using StudentStudyHub.Domain.Exceptions;

namespace StudentStudyHub.Application.UnitTests.StudyGroups;

public class StudyGroupServiceTests
{
    private static readonly Guid OwnerId = Guid.NewGuid();
    private static readonly Guid MemberId = Guid.NewGuid();
    private static readonly Guid OutsiderId = Guid.NewGuid();

    private static (StudyGroupService Service, FakeAppDbContext Db, FakeDateTimeProvider Clock) CreateSut()
    {
        var db = new FakeAppDbContext();
        var clock = new FakeDateTimeProvider();
        var service = new StudyGroupService(db, clock);
        return (service, db, clock);
    }

    private static CreateStudyGroupRequest ValidCreateRequest() =>
        new("AP Chemistry Crew", "Chemistry", "Lincoln High School");

    [Fact]
    public async Task CreateAsync_CreatesGroupAndOwnerMembership_AndFindOrCreatesSchool()
    {
        var (service, db, _) = CreateSut();

        var created = await service.CreateAsync(OwnerId, ValidCreateRequest());

        Assert.Single(db.StudyGroupsList);
        Assert.Single(db.SchoolsList);
        Assert.Single(db.StudyGroupMembersList);
        Assert.Equal("Lincoln High School", created.SchoolName);
        Assert.Equal(OwnerId, created.OwnerUserId);
        Assert.True(created.IsOwner);
        Assert.Equal(1, created.MemberCount);
    }

    [Fact]
    public async Task CreateAsync_WithExistingSchoolName_ReusesSameSchool()
    {
        var (service, db, _) = CreateSut();

        await service.CreateAsync(OwnerId, ValidCreateRequest());
        await service.CreateAsync(OwnerId, ValidCreateRequest() with { Name = "Second Group" });

        Assert.Single(db.SchoolsList);
        Assert.Equal(2, db.StudyGroupsList.Count);
    }

    [Fact]
    public async Task ListMineAsync_OnlyReturnsGroupsUserIsMemberOf()
    {
        var (service, _, _) = CreateSut();
        var mine = await service.CreateAsync(OwnerId, ValidCreateRequest());
        await service.CreateAsync(OutsiderId, ValidCreateRequest() with { Name = "Not mine" });

        var result = await service.ListMineAsync(OwnerId);

        Assert.Single(result);
        Assert.Equal(mine.Id, result[0].Id);
    }

    [Fact]
    public async Task JoinAsync_WithValidInviteCode_AddsMembership()
    {
        var (service, db, _) = CreateSut();
        var created = await service.CreateAsync(OwnerId, ValidCreateRequest());
        var inviteCode = db.StudyGroupsList[0].InviteCode;

        var joined = await service.JoinAsync(MemberId, new JoinStudyGroupRequest(inviteCode));

        Assert.Equal(created.Id, joined.Id);
        Assert.Equal(2, joined.MemberCount);
        Assert.False(joined.IsOwner);
    }

    [Fact]
    public async Task JoinAsync_WithUnknownInviteCode_ThrowsDomainException()
    {
        var (service, _, _) = CreateSut();

        await Assert.ThrowsAsync<DomainException>(() => service.JoinAsync(MemberId, new JoinStudyGroupRequest("ZZZZZZZZ")));
    }

    [Fact]
    public async Task JoinAsync_WhenAlreadyMember_ThrowsDomainException()
    {
        var (service, db, _) = CreateSut();
        await service.CreateAsync(OwnerId, ValidCreateRequest());
        var inviteCode = db.StudyGroupsList[0].InviteCode;

        await Assert.ThrowsAsync<DomainException>(() => service.JoinAsync(OwnerId, new JoinStudyGroupRequest(inviteCode)));
    }

    [Fact]
    public async Task GetAsync_ForNonMember_ThrowsNotFound()
    {
        var (service, _, _) = CreateSut();
        var created = await service.CreateAsync(OwnerId, ValidCreateRequest());

        await Assert.ThrowsAsync<NotFoundException>(() => service.GetAsync(OutsiderId, created.Id));
    }

    [Fact]
    public async Task UpdateAsync_ByNonOwnerMember_ThrowsForbidden()
    {
        var (service, db, _) = CreateSut();
        var created = await service.CreateAsync(OwnerId, ValidCreateRequest());
        var inviteCode = db.StudyGroupsList[0].InviteCode;
        await service.JoinAsync(MemberId, new JoinStudyGroupRequest(inviteCode));

        await Assert.ThrowsAsync<ForbiddenException>(() =>
            service.UpdateAsync(MemberId, created.Id, new UpdateStudyGroupRequest("New name", null)));
    }

    [Fact]
    public async Task UpdateAsync_ByOwner_UpdatesFields()
    {
        var (service, _, clock) = CreateSut();
        var created = await service.CreateAsync(OwnerId, ValidCreateRequest());
        clock.UtcNow = clock.UtcNow.AddHours(1);

        var updated = await service.UpdateAsync(OwnerId, created.Id, new UpdateStudyGroupRequest("Renamed", "Physics"));

        Assert.Equal("Renamed", updated.Name);
        Assert.Equal("Physics", updated.Subject);
    }

    [Fact]
    public async Task LeaveAsync_ByOwner_ThrowsDomainException()
    {
        var (service, _, _) = CreateSut();
        var created = await service.CreateAsync(OwnerId, ValidCreateRequest());

        await Assert.ThrowsAsync<DomainException>(() => service.LeaveAsync(OwnerId, created.Id));
    }

    [Fact]
    public async Task LeaveAsync_ByMember_RemovesMembership()
    {
        var (service, db, _) = CreateSut();
        var created = await service.CreateAsync(OwnerId, ValidCreateRequest());
        var inviteCode = db.StudyGroupsList[0].InviteCode;
        await service.JoinAsync(MemberId, new JoinStudyGroupRequest(inviteCode));

        await service.LeaveAsync(MemberId, created.Id);

        Assert.Single(db.StudyGroupMembersList);
    }

    [Fact]
    public async Task DeleteAsync_ByNonOwnerMember_ThrowsForbidden()
    {
        var (service, db, _) = CreateSut();
        var created = await service.CreateAsync(OwnerId, ValidCreateRequest());
        var inviteCode = db.StudyGroupsList[0].InviteCode;
        await service.JoinAsync(MemberId, new JoinStudyGroupRequest(inviteCode));

        await Assert.ThrowsAsync<ForbiddenException>(() => service.DeleteAsync(MemberId, created.Id));
    }

    [Fact]
    public async Task DeleteAsync_ByOwner_RemovesGroupMembersAndNotes()
    {
        var (service, db, _) = CreateSut();
        var created = await service.CreateAsync(OwnerId, ValidCreateRequest());
        var inviteCode = db.StudyGroupsList[0].InviteCode;
        await service.JoinAsync(MemberId, new JoinStudyGroupRequest(inviteCode));
        db.GroupNotesList.Add(new GroupNote { StudyGroupId = created.Id, AuthorUserId = OwnerId, Title = "Note" });

        await service.DeleteAsync(OwnerId, created.Id);

        Assert.Empty(db.StudyGroupsList);
        Assert.Empty(db.StudyGroupMembersList);
        Assert.Empty(db.GroupNotesList);
    }

    private class FakeAppDbContext : IAppDbContext
    {
        public List<StudyGroup> StudyGroupsList { get; } = new();
        public List<StudyGroupMember> StudyGroupMembersList { get; } = new();
        public List<GroupNote> GroupNotesList { get; } = new();
        public List<School> SchoolsList { get; } = new();
        public List<User> UsersList { get; } = new();

        public IQueryable<School> Schools => SchoolsList.AsQueryable();
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
                case StudyGroup group:
                    StudyGroupsList.Add(group);
                    return;
                case StudyGroupMember member:
                    StudyGroupMembersList.Add(member);
                    return;
                case GroupNote note:
                    GroupNotesList.Add(note);
                    return;
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
            switch (entity)
            {
                case StudyGroup group:
                    StudyGroupsList.Remove(group);
                    return;
                case StudyGroupMember member:
                    StudyGroupMembersList.Remove(member);
                    return;
                case GroupNote note:
                    GroupNotesList.Remove(note);
                    return;
                default:
                    throw new NotSupportedException($"FakeAppDbContext does not support removing {typeof(TEntity).Name}.");
            }
        }

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => Task.FromResult(0);
    }

    private class FakeDateTimeProvider : IDateTimeProvider
    {
        public DateTime UtcNow { get; set; } = DateTime.UtcNow;
    }
}
