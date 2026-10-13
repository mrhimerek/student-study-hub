using StudentStudyHub.Domain.Enums;

namespace StudentStudyHub.Application.Assignments;

public record AssignmentDto(
    Guid Id,
    string Subject,
    string Title,
    string? Description,
    DateTime DueDate,
    Priority Priority,
    AssignmentStatus Status,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public record CreateAssignmentRequest(
    string Subject,
    string Title,
    string? Description,
    DateTime DueDate,
    Priority Priority);

public record UpdateAssignmentRequest(
    string Subject,
    string Title,
    string? Description,
    DateTime DueDate,
    Priority Priority,
    AssignmentStatus Status);
