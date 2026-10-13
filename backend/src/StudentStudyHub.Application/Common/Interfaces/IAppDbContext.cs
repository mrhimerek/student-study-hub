using StudentStudyHub.Domain.Entities;

namespace StudentStudyHub.Application.Common.Interfaces;

/// <summary>
/// Abstraction over the EF Core DbContext so the Application layer never
/// depends directly on Microsoft.EntityFrameworkCore / Infrastructure.
/// </summary>
public interface IAppDbContext
{
    IQueryable<School> Schools { get; }
    IQueryable<User> Users { get; }
    IQueryable<Assignment> Assignments { get; }
    IQueryable<StudyGroup> StudyGroups { get; }
    IQueryable<StudyGroupMember> StudyGroupMembers { get; }
    IQueryable<GroupNote> GroupNotes { get; }
    IQueryable<Exam> Exams { get; }
    IQueryable<Course> Courses { get; }
    IQueryable<SatPlan> SatPlans { get; }
    IQueryable<SatPracticeScore> SatPracticeScores { get; }
    IQueryable<Scholarship> Scholarships { get; }
    IQueryable<CollegeApplication> CollegeApplications { get; }
    IQueryable<Notification> Notifications { get; }
    IQueryable<RefreshToken> RefreshTokens { get; }

    void Add<TEntity>(TEntity entity) where TEntity : class;
    void Remove<TEntity>(TEntity entity) where TEntity : class;

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
