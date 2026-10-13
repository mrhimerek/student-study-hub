using StudentStudyHub.Domain.Enums;

namespace StudentStudyHub.Application.Exams;

public record ExamDto(
    Guid Id,
    string Subject,
    string Title,
    DateTime ExamDate,
    ExamType Type,
    string? Notes,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public record CreateExamRequest(
    string Subject,
    string Title,
    DateTime ExamDate,
    ExamType Type,
    string? Notes);

public record UpdateExamRequest(
    string Subject,
    string Title,
    DateTime ExamDate,
    ExamType Type,
    string? Notes);
