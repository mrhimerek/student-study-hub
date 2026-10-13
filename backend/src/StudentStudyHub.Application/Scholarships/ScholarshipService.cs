using System.Text.Json;
using StudentStudyHub.Application.Common.Interfaces;
using StudentStudyHub.Domain.Entities;
using StudentStudyHub.Domain.Exceptions;

namespace StudentStudyHub.Application.Scholarships;

/// <summary>
/// CRUD for the scholarship tracker. Scoped by owning UserId like Assignments/Courses/Exams -
/// one student can never read or mutate another student's scholarships. The requirements
/// checklist is stored as a JSON string column (RequirementsJson) and (de)serialized here so the
/// rest of the app only ever deals with a plain string list.
/// </summary>
public class ScholarshipService(IAppDbContext db, IDateTimeProvider clock) : IScholarshipService
{
    public Task<IReadOnlyList<ScholarshipDto>> ListAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var scholarships = db.Scholarships.Where(s => s.UserId == userId).OrderBy(s => s.Deadline).ToList();
        IReadOnlyList<ScholarshipDto> result = scholarships.Select(ToDto).ToList();
        return Task.FromResult(result);
    }

    public Task<ScholarshipDto> GetAsync(Guid userId, Guid scholarshipId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(ToDto(FindOwned(userId, scholarshipId)));
    }

    public async Task<ScholarshipDto> CreateAsync(Guid userId, CreateScholarshipRequest request, CancellationToken cancellationToken = default)
    {
        var scholarship = new Scholarship
        {
            UserId = userId,
            Name = request.Name.Trim(),
            Amount = request.Amount,
            Deadline = request.Deadline,
            Status = request.Status,
            RequirementsJson = SerializeRequirements(request.Requirements),
        };

        db.Add(scholarship);
        await db.SaveChangesAsync(cancellationToken);

        return ToDto(scholarship);
    }

    public async Task<ScholarshipDto> UpdateAsync(Guid userId, Guid scholarshipId, UpdateScholarshipRequest request, CancellationToken cancellationToken = default)
    {
        var scholarship = FindOwned(userId, scholarshipId);

        scholarship.Name = request.Name.Trim();
        scholarship.Amount = request.Amount;
        scholarship.Deadline = request.Deadline;
        scholarship.Status = request.Status;
        scholarship.RequirementsJson = SerializeRequirements(request.Requirements);
        scholarship.UpdatedAt = clock.UtcNow;

        await db.SaveChangesAsync(cancellationToken);

        return ToDto(scholarship);
    }

    public async Task DeleteAsync(Guid userId, Guid scholarshipId, CancellationToken cancellationToken = default)
    {
        var scholarship = FindOwned(userId, scholarshipId);
        db.Remove(scholarship);
        await db.SaveChangesAsync(cancellationToken);
    }

    private Scholarship FindOwned(Guid userId, Guid scholarshipId)
    {
        return db.Scholarships.FirstOrDefault(s => s.Id == scholarshipId && s.UserId == userId)
            ?? throw new NotFoundException(nameof(Scholarship), scholarshipId);
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

    private static ScholarshipDto ToDto(Scholarship s) => new(
        s.Id, s.Name, s.Amount, s.Deadline, s.Status, DeserializeRequirements(s.RequirementsJson), s.CreatedAt, s.UpdatedAt);
}
