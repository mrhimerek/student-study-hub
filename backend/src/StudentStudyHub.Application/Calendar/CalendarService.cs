using System.Text;
using StudentStudyHub.Application.Common.Interfaces;

namespace StudentStudyHub.Application.Calendar;

public class CalendarService(IAppDbContext db) : ICalendarService
{
    public Task<IReadOnlyList<CalendarEventDto>> GetEventsAsync(
        Guid userId, DateOnly from, DateOnly to, CancellationToken cancellationToken = default)
    {
        var events = new List<CalendarEventDto>();

        var assignments = db.Assignments.Where(a => a.UserId == userId).ToList();
        foreach (var a in assignments)
        {
            var date = DateOnly.FromDateTime(a.DueDate);
            if (date >= from && date <= to)
            {
                events.Add(new CalendarEventDto(a.Id, CalendarEventType.Assignment, a.Title, a.Subject, date));
            }
        }

        var exams = db.Exams.Where(e => e.UserId == userId).ToList();
        foreach (var e in exams)
        {
            var date = DateOnly.FromDateTime(e.ExamDate);
            if (date >= from && date <= to)
            {
                events.Add(new CalendarEventDto(e.Id, CalendarEventType.Exam, e.Title, e.Subject, date));
            }
        }

        var scholarships = db.Scholarships.Where(s => s.UserId == userId && s.Deadline >= from && s.Deadline <= to).ToList();
        foreach (var s in scholarships)
        {
            events.Add(new CalendarEventDto(s.Id, CalendarEventType.Scholarship, s.Name, Humanize(s.Status.ToString()), s.Deadline));
        }

        var applications = db.CollegeApplications.Where(c => c.UserId == userId && c.Deadline >= from && c.Deadline <= to).ToList();
        foreach (var c in applications)
        {
            events.Add(new CalendarEventDto(c.Id, CalendarEventType.CollegeApplication, c.CollegeName, Humanize(c.ApplicationType.ToString()), c.Deadline));
        }

        var satPlan = db.SatPlans.FirstOrDefault(p => p.UserId == userId && p.TargetTestDate >= from && p.TargetTestDate <= to);
        if (satPlan is not null)
        {
            events.Add(new CalendarEventDto(satPlan.Id, CalendarEventType.SatTarget, "SAT Test Date", $"Goal {satPlan.GoalScore}", satPlan.TargetTestDate));
        }

        IReadOnlyList<CalendarEventDto> result = events.OrderBy(e => e.Date).ThenBy(e => e.Type).ToList();
        return Task.FromResult(result);
    }

    /// <summary>Turns a PascalCase enum name like "EarlyDecision" into "Early Decision".</summary>
    private static string Humanize(string pascalCase)
    {
        var sb = new StringBuilder();
        foreach (var c in pascalCase)
        {
            if (sb.Length > 0 && char.IsUpper(c))
            {
                sb.Append(' ');
            }

            sb.Append(c);
        }

        return sb.ToString();
    }
}
