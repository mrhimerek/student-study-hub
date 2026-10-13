using StudentStudyHub.Application.Auth;
using StudentStudyHub.Application.Common.Interfaces;
using StudentStudyHub.Application.Common.Settings;
using StudentStudyHub.Domain.Entities;
using StudentStudyHub.Domain.Exceptions;

namespace StudentStudyHub.Application.UnitTests.Auth;

public class AuthServiceTests
{
    private static (AuthService Service, FakeAppDbContext Db, FakeDateTimeProvider Clock) CreateSut()
    {
        var db = new FakeAppDbContext();
        var clock = new FakeDateTimeProvider();
        var settings = new JwtSettings { Secret = "test-secret", AccessTokenMinutes = 15, RefreshTokenDays = 30 };
        var service = new AuthService(db, new FakePasswordHasher(), new FakeJwtTokenService(), clock, settings);
        return (service, db, clock);
    }

    private static RegisterRequest ValidRegisterRequest(string email = "student@example.com") =>
        new(email, "P@ssword1", "Ada", "Lovelace", 10, null);

    [Fact]
    public async Task RegisterAsync_CreatesUser_AndReturnsTokens()
    {
        var (service, db, _) = CreateSut();

        var result = await service.RegisterAsync(ValidRegisterRequest());

        Assert.Single(db.UsersList);
        Assert.Equal("student@example.com", result.User.Email);
        Assert.False(string.IsNullOrWhiteSpace(result.AccessToken));
        Assert.False(string.IsNullOrWhiteSpace(result.RefreshToken));
        Assert.Single(db.RefreshTokensList);
    }

    [Fact]
    public async Task RegisterAsync_NormalizesEmailCasing_AndRejectsDuplicates()
    {
        var (service, _, _) = CreateSut();

        await service.RegisterAsync(ValidRegisterRequest("Student@Example.com"));

        await Assert.ThrowsAsync<DomainException>(() =>
            service.RegisterAsync(ValidRegisterRequest("student@example.com")));
    }

    [Fact]
    public async Task LoginAsync_WithCorrectPassword_ReturnsTokens()
    {
        var (service, _, _) = CreateSut();
        await service.RegisterAsync(ValidRegisterRequest());

        var result = await service.LoginAsync(new LoginRequest("student@example.com", "P@ssword1"));

        Assert.Equal("student@example.com", result.User.Email);
    }

    [Fact]
    public async Task LoginAsync_WithWrongPassword_Throws()
    {
        var (service, _, _) = CreateSut();
        await service.RegisterAsync(ValidRegisterRequest());

        await Assert.ThrowsAsync<DomainException>(() =>
            service.LoginAsync(new LoginRequest("student@example.com", "WrongPassword1")));
    }

    [Fact]
    public async Task LoginAsync_WithUnknownEmail_Throws()
    {
        var (service, _, _) = CreateSut();

        await Assert.ThrowsAsync<DomainException>(() =>
            service.LoginAsync(new LoginRequest("ghost@example.com", "P@ssword1")));
    }

    [Fact]
    public async Task RefreshAsync_RotatesToken_RevokingTheOldOne()
    {
        var (service, db, _) = CreateSut();
        var registered = await service.RegisterAsync(ValidRegisterRequest());

        var refreshed = await service.RefreshAsync(registered.RefreshToken);

        Assert.NotEqual(registered.RefreshToken, refreshed.RefreshToken);
        Assert.Equal(2, db.RefreshTokensList.Count);
        Assert.Contains(db.RefreshTokensList, rt => rt.RevokedAt is not null);
        Assert.Contains(db.RefreshTokensList, rt => rt.IsActive);
    }

    [Fact]
    public async Task RefreshAsync_WithUnknownToken_Throws()
    {
        var (service, _, _) = CreateSut();

        await Assert.ThrowsAsync<DomainException>(() => service.RefreshAsync("not-a-real-token"));
    }

    [Fact]
    public async Task RefreshAsync_WithAlreadyUsedToken_Throws()
    {
        var (service, _, _) = CreateSut();
        var registered = await service.RegisterAsync(ValidRegisterRequest());
        await service.RefreshAsync(registered.RefreshToken);

        await Assert.ThrowsAsync<DomainException>(() => service.RefreshAsync(registered.RefreshToken));
    }

    [Fact]
    public async Task LogoutAsync_RevokesActiveToken()
    {
        var (service, db, _) = CreateSut();
        var registered = await service.RegisterAsync(ValidRegisterRequest());

        await service.LogoutAsync(registered.RefreshToken);

        Assert.All(db.RefreshTokensList, rt => Assert.False(rt.IsActive));
    }

    private class FakeAppDbContext : IAppDbContext
    {
        public List<User> UsersList { get; } = new();
        public List<RefreshToken> RefreshTokensList { get; } = new();

        public IQueryable<School> Schools => new List<School>().AsQueryable();
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
        public IQueryable<RefreshToken> RefreshTokens => RefreshTokensList.AsQueryable();

        public void Add<TEntity>(TEntity entity) where TEntity : class
        {
            switch (entity)
            {
                case User user:
                    UsersList.Add(user);
                    break;
                case RefreshToken token:
                    RefreshTokensList.Add(token);
                    break;
                default:
                    throw new NotSupportedException($"FakeAppDbContext does not support adding {typeof(TEntity).Name}.");
            }
        }

        public void Remove<TEntity>(TEntity entity) where TEntity : class
        {
            switch (entity)
            {
                case User user:
                    UsersList.Remove(user);
                    break;
                case RefreshToken token:
                    RefreshTokensList.Remove(token);
                    break;
                default:
                    throw new NotSupportedException($"FakeAppDbContext does not support removing {typeof(TEntity).Name}.");
            }
        }

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => Task.FromResult(0);
    }

    private class FakeDateTimeProvider : IDateTimeProvider
    {
        // RefreshToken.IsActive compares against the real DateTime.UtcNow (not an injected clock),
        // so the fake must track real time rather than a fixed date to avoid spurious "expired" tokens.
        public DateTime UtcNow { get; set; } = DateTime.UtcNow;
    }

    private class FakePasswordHasher : IPasswordHasher
    {
        public string Hash(string password) => $"hashed:{password}";

        public bool Verify(string password, string passwordHash) => passwordHash == $"hashed:{password}";
    }

    private class FakeJwtTokenService : IJwtTokenService
    {
        private int _refreshTokenCounter;

        public string GenerateAccessToken(Guid userId, string email) => $"access-token:{userId}:{email}";

        public string GenerateRefreshToken() => $"refresh-token:{Interlocked.Increment(ref _refreshTokenCounter)}:{Guid.NewGuid()}";

        public System.Security.Claims.ClaimsPrincipal? ValidateAccessToken(string token) => null;
    }
}
