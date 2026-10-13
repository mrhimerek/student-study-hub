using StudentStudyHub.Application.Common.Interfaces;
using StudentStudyHub.Domain.Entities;
using StudentStudyHub.Domain.Exceptions;

namespace StudentStudyHub.Application.Courses;

/// <summary>
/// CRUD for the courses that feed the GPA calculator. Every query is scoped to the owning
/// user's id (never a bare course id), mirroring AssignmentService - one student can never
/// read or mutate another student's courses.
/// </summary>
public class CourseService(IAppDbContext db, IDateTimeProvider clock) : ICourseService
{
    public Task<IReadOnlyList<CourseDto>> ListAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        // Materialize before mapping: IAppDbContext's queryables are backed by EF Core in
        // production, which cannot translate an arbitrary C# mapping method into SQL.
        var courses = db.Courses.Where(c => c.UserId == userId).OrderBy(c => c.Term).ThenBy(c => c.Name).ToList();
        IReadOnlyList<CourseDto> result = courses.Select(ToDto).ToList();
        return Task.FromResult(result);
    }

    public Task<CourseDto> GetAsync(Guid userId, Guid courseId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(ToDto(FindOwned(userId, courseId)));
    }

    public async Task<CourseDto> CreateAsync(Guid userId, CreateCourseRequest request, CancellationToken cancellationToken = default)
    {
        var course = new Course
        {
            UserId = userId,
            Name = request.Name.Trim(),
            CreditHours = request.CreditHours,
            GradeValue = request.GradeValue.Trim(),
            IsHonorsOrAp = request.IsHonorsOrAp,
            Term = request.Term.Trim(),
        };

        db.Add(course);
        await db.SaveChangesAsync(cancellationToken);

        return ToDto(course);
    }

    public async Task<CourseDto> UpdateAsync(Guid userId, Guid courseId, UpdateCourseRequest request, CancellationToken cancellationToken = default)
    {
        var course = FindOwned(userId, courseId);

        course.Name = request.Name.Trim();
        course.CreditHours = request.CreditHours;
        course.GradeValue = request.GradeValue.Trim();
        course.IsHonorsOrAp = request.IsHonorsOrAp;
        course.Term = request.Term.Trim();
        course.UpdatedAt = clock.UtcNow;

        await db.SaveChangesAsync(cancellationToken);

        return ToDto(course);
    }

    public async Task DeleteAsync(Guid userId, Guid courseId, CancellationToken cancellationToken = default)
    {
        var course = FindOwned(userId, courseId);
        db.Remove(course);
        await db.SaveChangesAsync(cancellationToken);
    }

    private Course FindOwned(Guid userId, Guid courseId)
    {
        return db.Courses.FirstOrDefault(c => c.Id == courseId && c.UserId == userId)
            ?? throw new NotFoundException(nameof(Course), courseId);
    }

    private static CourseDto ToDto(Course c) => new(
        c.Id, c.Name, c.CreditHours, c.GradeValue, c.IsHonorsOrAp, c.Term, c.CreatedAt, c.UpdatedAt);
}
