using System.Text.Json;
using StudentStudyHub.Application.Common.Interfaces;
using StudentStudyHub.Domain.Entities;
using StudentStudyHub.Domain.Exceptions;

namespace StudentStudyHub.Application.CollegeApplications;

/// <summary>
/// CRUD for the college application tracker. Scoped by owning UserId like
/// Assignments/Courses/Exams/Scholarships. The requirements checklist is stored as a JSON string
/// column (RequirementsJson) and (de)serialized here, same approach as ScholarshipService.
/// </summary>
public class CollegeApplicationService(IAppDbContext db, IDateTimeProvider clock) : ICollegeApplicationService
{
    public Task<IReadOnlyList<CollegeApplicationDto>> ListAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var applications = db.CollegeApplications.Where(a => a.UserId == userId).OrderBy(a => a.Deadline).ToList();
        IReadOnlyList<CollegeApplicationDto> result = applications.Select(ToDto).ToList();
        return Task.FromResult(result);
    }

    public Task<CollegeApplicationDto> GetAsync(Guid userId, Guid applicationId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(ToDto(FindOwned(userId, applicationId)));
    }

    public async Task<CollegeApplicationDto> CreateAsync(Guid userId, CreateCollegeApplicationRequest request, CancellationToken cancellationToken = default)
    {
        var application = new CollegeApplication
        {
            UserId = userId,
            CollegeName = request.CollegeName.Trim(),
            ApplicationType = request.ApplicationType,
            Deadline = request.Deadline,
            Status = request.Status,
            RequirementsJson = SerializeRequirements(request.Requirements),
        };

        db.Add(application);
        await db.SaveChangesAsync(cancellationToken);

        return ToDto(application);
    }

    public async Task<CollegeApplicationDto> UpdateAsync(Guid userId, Guid applicationId, UpdateCollegeApplicationRequest request, CancellationToken cancellationToken = default)
    {
        var application = FindOwned(userId, applicationId);

        application.CollegeName = request.CollegeName.Trim();
        application.ApplicationType = request.ApplicationType;
        application.Deadline = request.Deadline;
        application.Status = request.Status;
        application.RequirementsJson = SerializeRequirements(request.Requirements);
        application.UpdatedAt = clock.UtcNow;

        await db.SaveChangesAsync(cancellationToken);

        return ToDto(application);
    }

    public async Task DeleteAsync(Guid userId, Guid applicationId, CancellationToken cancellationToken = default)
    {
        var application = FindOwned(userId, applicationId);
        db.Remove(application);
        await db.SaveChangesAsync(cancellationToken);
    }

    private CollegeApplication FindOwned(Guid userId, Guid applicationId)
    {
        return db.CollegeApplications.FirstOrDefault(a => a.Id == applicationId && a.UserId == userId)
            ?? throw new NotFoundException(nameof(CollegeApplication), applicationId);
    }

    private static string? SerializeRequirements(IReadOnlyList<string>? requirements)
    {
        var trimmed = (requirements ?? []).Select(r => r.Trim()).Where(r => r.Length > 0).ToList();
        return trimmed.Count == 0 ? null : JsonSerializer.Serialize(trimmed);
    }

    private static IReadOnlyList<string> DeserializeRequirements(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return [];
        }

        return JsonSerializer.Deserialize<List<string>>(json) ?? [];
    }

    private static CollegeApplicationDto ToDto(CollegeApplication a) => new(
        a.Id, a.CollegeName, a.ApplicationType, a.Deadline, a.Status, DeserializeRequirements(a.RequirementsJson), a.CreatedAt, a.UpdatedAt);
}
