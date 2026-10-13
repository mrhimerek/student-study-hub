namespace StudentStudyHub.Application.Courses;

public record CourseDto(
    Guid Id,
    string Name,
    decimal CreditHours,
    string GradeValue,
    bool IsHonorsOrAp,
    string Term,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public record CreateCourseRequest(
    string Name,
    decimal CreditHours,
    string GradeValue,
    bool IsHonorsOrAp,
    string Term);

public record UpdateCourseRequest(
    string Name,
    decimal CreditHours,
    string GradeValue,
    bool IsHonorsOrAp,
    string Term);
