namespace StudentStudyHub.Application.Users;

public record ProfileDto(
    Guid Id,
    string Email,
    string FirstName,
    string LastName,
    int GradeLevel,
    string? SchoolName,
    string? AvatarUrl,
    DateTime CreatedAt);

/// <summary>
/// SchoolName is matched against existing schools by exact trimmed name, and a new School row is
/// created on first use (same find-or-create pattern as CreateStudyGroupRequest) - there is no
/// separate School CRUD/picker UI yet (see REMAINING_WORK.md). Passing null/empty clears the
/// student's school association.
/// </summary>
public record UpdateProfileRequest(string FirstName, string LastName, int GradeLevel, string? SchoolName);
