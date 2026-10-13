namespace StudentStudyHub.Application.Calendar;

public enum CalendarEventType
{
    Assignment,
    Exam,
    Scholarship,
    CollegeApplication,
    SatTarget,
}

public record CalendarEventDto(
    Guid SourceId,
    CalendarEventType Type,
    string Title,
    string? Subtitle,
    DateOnly Date);
