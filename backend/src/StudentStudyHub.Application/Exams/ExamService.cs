using StudentStudyHub.Application.Common.Interfaces;
using StudentStudyHub.Domain.Entities;
using StudentStudyHub.Domain.Exceptions;

namespace StudentStudyHub.Application.Exams;

/// <summary>
/// CRUD for the exam/quiz countdown tracker. Every query is scoped to the owning user's id
/// (never a bare exam id), mirroring AssignmentService/CourseService - one student can never
/// read or mutate another student's exams.
/// </summary>
public class ExamService(IAppDbContext db, IDateTimeProvider clock) : IExamService
{
    public Task<IReadOnlyList<ExamDto>> ListAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        // Materialize before mapping: IAppDbContext's queryables are backed by EF Core in
        // production, which cannot translate an arbitrary C# mapping method into SQL.
        var exams = db.Exams.Where(e => e.UserId == userId).OrderBy(e => e.ExamDate).ToList();
        IReadOnlyList<ExamDto> result = exams.Select(ToDto).ToList();
        return Task.FromResult(result);
    }

    public Task<ExamDto?> GetNextAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var now = clock.UtcNow;
        var next = db.Exams
            .Where(e => e.UserId == userId && e.ExamDate >= now)
            .OrderBy(e => e.ExamDate)
            .FirstOrDefault();

        return Task.FromResult(next is null ? null : ToDto(next));
    }

    public Task<ExamDto> GetAsync(Guid userId, Guid examId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(ToDto(FindOwned(userId, examId)));
    }

    public async Task<ExamDto> CreateAsync(Guid userId, CreateExamRequest request, CancellationToken cancellationToken = default)
    {
        var exam = new Exam
        {
            UserId = userId,
            Subject = request.Subject.Trim(),
            Title = request.Title.Trim(),
            ExamDate = request.ExamDate,
            Type = request.Type,
            Notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim(),
        };

        db.Add(exam);
        await db.SaveChangesAsync(cancellationToken);

        return ToDto(exam);
    }

    public async Task<ExamDto> UpdateAsync(Guid userId, Guid examId, UpdateExamRequest request, CancellationToken cancellationToken = default)
    {
        var exam = FindOwned(userId, examId);

        exam.Subject = request.Subject.Trim();
        exam.Title = request.Title.Trim();
        exam.ExamDate = request.ExamDate;
        exam.Type = request.Type;
        exam.Notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim();
        exam.UpdatedAt = clock.UtcNow;

        await db.SaveChangesAsync(cancellationToken);

        return ToDto(exam);
    }

    public async Task DeleteAsync(Guid userId, Guid examId, CancellationToken cancellationToken = default)
    {
        var exam = FindOwned(userId, examId);
        db.Remove(exam);
        await db.SaveChangesAsync(cancellationToken);
    }

    private Exam FindOwned(Guid userId, Guid examId)
    {
        return db.Exams.FirstOrDefault(e => e.Id == examId && e.UserId == userId)
            ?? throw new NotFoundException(nameof(Exam), examId);
    }

    private static ExamDto ToDto(Exam e) => new(
        e.Id, e.Subject, e.Title, e.ExamDate, e.Type, e.Notes, e.CreatedAt, e.UpdatedAt);
}
