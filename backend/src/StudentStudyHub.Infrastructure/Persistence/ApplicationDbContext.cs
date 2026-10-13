using Microsoft.EntityFrameworkCore;
using StudentStudyHub.Application.Common.Interfaces;
using StudentStudyHub.Domain.Entities;

namespace StudentStudyHub.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext, IAppDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<School> SchoolsSet => Set<School>();
    public DbSet<User> UsersSet => Set<User>();
    public DbSet<Assignment> AssignmentsSet => Set<Assignment>();
    public DbSet<StudyGroup> StudyGroupsSet => Set<StudyGroup>();
    public DbSet<StudyGroupMember> StudyGroupMembersSet => Set<StudyGroupMember>();
    public DbSet<GroupNote> GroupNotesSet => Set<GroupNote>();
    public DbSet<Exam> ExamsSet => Set<Exam>();
    public DbSet<Course> CoursesSet => Set<Course>();
    public DbSet<SatPlan> SatPlansSet => Set<SatPlan>();
    public DbSet<SatPracticeScore> SatPracticeScoresSet => Set<SatPracticeScore>();
    public DbSet<Scholarship> ScholarshipsSet => Set<Scholarship>();
    public DbSet<CollegeApplication> CollegeApplicationsSet => Set<CollegeApplication>();
    public DbSet<Notification> NotificationsSet => Set<Notification>();
    public DbSet<RefreshToken> RefreshTokensSet => Set<RefreshToken>();

    // IAppDbContext projections used by the Application layer (no direct EF Core dependency there).
    public IQueryable<School> Schools => SchoolsSet;
    public IQueryable<User> Users => UsersSet;
    public IQueryable<Assignment> Assignments => AssignmentsSet;
    public IQueryable<StudyGroup> StudyGroups => StudyGroupsSet;
    public IQueryable<StudyGroupMember> StudyGroupMembers => StudyGroupMembersSet;
    public IQueryable<GroupNote> GroupNotes => GroupNotesSet;
    public IQueryable<Exam> Exams => ExamsSet;
    public IQueryable<Course> Courses => CoursesSet;
    public IQueryable<SatPlan> SatPlans => SatPlansSet;
    public IQueryable<SatPracticeScore> SatPracticeScores => SatPracticeScoresSet;
    public IQueryable<Scholarship> Scholarships => ScholarshipsSet;
    public IQueryable<CollegeApplication> CollegeApplications => CollegeApplicationsSet;
    public IQueryable<Notification> Notifications => NotificationsSet;
    public IQueryable<RefreshToken> RefreshTokens => RefreshTokensSet;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
