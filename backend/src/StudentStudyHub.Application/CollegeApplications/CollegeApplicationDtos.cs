using StudentStudyHub.Domain.Enums;

namespace StudentStudyHub.Application.CollegeApplications;

public record CollegeApplicationDto(
    Guid Id,
    string CollegeName,
    ApplicationType ApplicationType,
    DateOnly Deadline,
    CollegeApplicationStatus Status,
    IReadOnlyList<string> Requirements,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public record CreateCollegeApplicationRequest(
    string CollegeName,
    ApplicationType ApplicationType,
    DateOnly Deadline,
    CollegeApplicationStatus Status,
    IReadOnlyList<string>? Requirements);

public record UpdateCollegeApplicationRequest(
    string CollegeName,
    ApplicationType ApplicationType,
    DateOnly Deadline,
    CollegeApplicationStatus Status,
    IReadOnlyList<string>? Requirements);
