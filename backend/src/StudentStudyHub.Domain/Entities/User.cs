using StudentStudyHub.Domain.Common;

namespace StudentStudyHub.Domain.Entities;

public class User : BaseEntity
{
    public Guid? SchoolId { get; set; }
    public School? School { get; set; }

    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public int GradeLevel { get; set; } // 9-12
    public string? AvatarUrl { get; set; }
    public bool EmailVerified { get; set; }

    public ICollection<Assignment> Assignments { get; set; } = new List<Assignment>();
    public ICollection<Exam> Exams { get; set; } = new List<Exam>();
    public ICollection<Course> Courses { get; set; } = new List<Course>();
    public ICollection<Scholarship> Scholarships { get; set; } = new List<Scholarship>();
    public ICollection<CollegeApplication> CollegeApplications { get; set; } = new List<CollegeApplication>();
    public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    public ICollection<StudyGroupMember> StudyGroupMemberships { get; set; } = new List<StudyGroupMember>();
    public SatPlan? SatPlan { get; set; }
}
