namespace StudentStudyHub.Application.Calendar;

/// <summary>
/// Aggregates every date-bound entity the student owns (Assignments, Exams, Scholarships,
/// College Applications, and their SAT target test date) into a single unified list of
/// calendar events for a date range, per FR-11 in DESIGN.md.
/// </summary>
public interface ICalendarService
{
    Task<IReadOnlyList<CalendarEventDto>> GetEventsAsync(
        Guid userId, DateOnly from, DateOnly to, CancellationToken cancellationToken = default);
}
