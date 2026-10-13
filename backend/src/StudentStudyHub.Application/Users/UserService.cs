using StudentStudyHub.Application.Common.Interfaces;
using StudentStudyHub.Domain.Entities;
using StudentStudyHub.Domain.Exceptions;

namespace StudentStudyHub.Application.Users;

/// <summary>
/// Profile view/edit for the current user. Unlike every other service (scoped by a userId
/// parameter to one of *that user's* resources), here the userId *is* the resource - there's
/// no separate id route segment, mirroring the single-record-per-user shape of SatPlanService.
/// </summary>
public class UserService(IAppDbContext db, IDateTimeProvider clock) : IUserService
{
    public Task<ProfileDto> GetProfileAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(ToDto(FindUser(userId)));
    }

    public async Task<ProfileDto> UpdateProfileAsync(
        Guid userId,
        UpdateProfileRequest request,
        CancellationToken cancellationToken = default)
    {
        var user = FindUser(userId);

        user.FirstName = request.FirstName.Trim();
        user.LastName = request.LastName.Trim();
        user.GradeLevel = request.GradeLevel;
        user.SchoolId = ResolveSchoolId(request.SchoolName);
        user.UpdatedAt = clock.UtcNow;

        await db.SaveChangesAsync(cancellationToken);

        return ToDto(user);
    }

    private User FindUser(Guid userId)
    {
        return db.Users.FirstOrDefault(u => u.Id == userId)
            ?? throw new NotFoundException(nameof(User), userId);
    }

    private Guid? ResolveSchoolId(string? schoolName)
    {
        var trimmed = schoolName?.Trim();
        if (string.IsNullOrEmpty(trimmed))
        {
            return null;
        }

        var school = db.Schools.FirstOrDefault(s => s.Name == trimmed);
        if (school is null)
        {
            school = new School { Name = trimmed };
            db.Add(school);
        }

        return school.Id;
    }

    private ProfileDto ToDto(User user)
    {
        var schoolName = user.SchoolId is { } schoolId
            ? db.Schools.FirstOrDefault(s => s.Id == schoolId)?.Name
            : null;

        return new ProfileDto(
            user.Id,
            user.Email,
            user.FirstName,
            user.LastName,
            user.GradeLevel,
            schoolName,
            user.AvatarUrl,
            user.CreatedAt);
    }
}
