using StudentStudyHub.Domain.Enums;

namespace StudentStudyHub.Application.Scholarships;

public record ScholarshipDto(
    Guid Id,
    string Name,
    decimal? Amount,
    DateOnly Deadline,
    ScholarshipStatus Status,
    IReadOnlyList<string> Requirements,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public record CreateScholarshipRequest(
    string Name,
    decimal? Amount,
    DateOnly Deadline,
    ScholarshipStatus Status,
    IReadOnlyList<string>? Requirements);

public record UpdateScholarshipRequest(
    string Name,
    decimal? Amount,
    DateOnly Deadline,
    ScholarshipStatus Status,
    IReadOnlyList<string>? Requirements);
