using StudentStudyHub.Application.Common.Interfaces;
using StudentStudyHub.Domain.Entities;
using StudentStudyHub.Domain.Enums;
using StudentStudyHub.Domain.Exceptions;

namespace StudentStudyHub.Application.Assignments;

/// <summary>
/// CRUD for the homework/assignment tracker. Every query is scoped to the owning user's id
/// (never a bare assignment id), so one student can never read or mutate another student's
/// assignments - attempting to access an assignment you don't own looks identical to it not
/// existing (NotFoundException), which avoids leaking which ids are valid.
/// </summary>
public class AssignmentService(IAppDbContext db, IDateTimeProvider clock) : IAssignmentService
{
    public Task<IReadOnlyList<AssignmentDto>> ListAsync(Guid userId, AssignmentStatus? status, CancellationToken cancellationToken = default)
    {
        var query = db.Assignments.Where(a => a.UserId == userId);
        if (status is { } s)
        {
            query = query.Where(a => a.Status == s);
        }

        // Materialize before mapping: IAppDbContext's queryables are backed by EF Core in
        // production, which cannot translate an arbitrary C# mapping method into SQL.
        var assignments = query.OrderBy(a => a.DueDate).ToList();
        IReadOnlyList<AssignmentDto> result = assignments.Select(ToDto).ToList();
        return Task.FromResult(result);
    }

    public Task<AssignmentDto> GetAsync(Guid userId, Guid assignmentId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(ToDto(FindOwned(userId, assignmentId)));
    }

    public async Task<AssignmentDto> CreateAsync(Guid userId, CreateAssignmentRequest request, CancellationToken cancellationToken = default)
    {
        var assignment = new Assignment
        {
            UserId = userId,
            Subject = request.Subject.Trim(),
            Title = request.Title.Trim(),
            Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim(),
            DueDate = request.DueDate,
            Priority = request.Priority,
            Status = AssignmentStatus.NotStarted,
        };

        db.Add(assignment);
        await db.SaveChangesAsync(cancellationToken);

        return ToDto(assignment);
    }

    public async Task<AssignmentDto> UpdateAsync(Guid userId, Guid assignmentId, UpdateAssignmentRequest request, CancellationToken cancellationToken = default)
    {
        var assignment = FindOwned(userId, assignmentId);

        assignment.Subject = request.Subject.Trim();
        assignment.Title = request.Title.Trim();
        assignment.Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();
        assignment.DueDate = request.DueDate;
        assignment.Priority = request.Priority;
        assignment.Status = request.Status;
        assignment.UpdatedAt = clock.UtcNow;

        await db.SaveChangesAsync(cancellationToken);

        return ToDto(assignment);
    }

    public async Task DeleteAsync(Guid userId, Guid assignmentId, CancellationToken cancellationToken = default)
    {
        var assignment = FindOwned(userId, assignmentId);
        db.Remove(assignment);
        await db.SaveChangesAsync(cancellationToken);
    }

    private Assignment FindOwned(Guid userId, Guid assignmentId)
    {
        return db.Assignments.FirstOrDefault(a => a.Id == assignmentId && a.UserId == userId)
            ?? throw new NotFoundException(nameof(Assignment), assignmentId);
    }

    private static AssignmentDto ToDto(Assignment a) => new(
        a.Id, a.Subject, a.Title, a.Description, a.DueDate, a.Priority, a.Status, a.CreatedAt, a.UpdatedAt);
}
