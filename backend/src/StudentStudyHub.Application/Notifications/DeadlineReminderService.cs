using Microsoft.Extensions.Logging;
using StudentStudyHub.Application.Common.Interfaces;
using StudentStudyHub.Domain.Entities;
using StudentStudyHub.Domain.Enums;

namespace StudentStudyHub.Application.Notifications;

/// <summary>
/// Core scan logic for the hourly DeadlineReminderFunction, pulled out into the Application
/// layer (like every other feature's service) so it's unit-testable without needing the real
/// TimerTrigger/Azurite infrastructure.
/// </summary>
public class DeadlineReminderService(IAppDbContext db, IEmailService emailService, IDateTimeProvider clock, ILogger<DeadlineReminderService> logger)
    : IDeadlineReminderService
{
    private static readonly int[] ReminderThresholdDays = [3, 1];

    public async Task<int> ScanAndNotifyAsync(CancellationToken cancellationToken = default)
    {
        var today = DateOnly.FromDateTime(clock.UtcNow);
        var todayUtc = clock.UtcNow.Date;

        var users = db.Users.ToList().ToDictionary(u => u.Id, u => u);
        var candidates = CollectCandidates(today).ToList();

        var created = 0;
        foreach (var candidate in candidates)
        {
            var alreadyNotified = db.Notifications.Any(n =>
                n.UserId == candidate.UserId &&
                n.Type == candidate.Type &&
                n.RelatedEntityId == candidate.RelatedEntityId &&
                n.CreatedAt.Date == todayUtc);

            if (alreadyNotified)
            {
                continue;
            }

            var notification = new Notification
            {
                UserId = candidate.UserId,
                Type = candidate.Type,
                Title = candidate.Title,
                Body = candidate.Body,
                RelatedEntityId = candidate.RelatedEntityId,
                IsRead = false,
                SentViaEmail = false,
                CreatedAt = clock.UtcNow,
            };

            db.Add(notification);
            created++;

            if (users.TryGetValue(candidate.UserId, out var user) && !string.IsNullOrWhiteSpace(user.Email))
            {
                try
                {
                    await emailService.SendEmailAsync(user.Email, candidate.Title, $"<p>{candidate.Body}</p>", cancellationToken);
                    notification.SentViaEmail = true;
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "Failed to send reminder email to {Email} for {Type} {RelatedEntityId}", user.Email, candidate.Type, candidate.RelatedEntityId);
                }
            }
        }

        if (created > 0)
        {
            await db.SaveChangesAsync(cancellationToken);
        }

        return created;
    }

    private IEnumerable<ReminderCandidate> CollectCandidates(DateOnly today)
    {
        foreach (var days in ReminderThresholdDays)
        {
            var targetDate = today.AddDays(days);
            var dayWord = days == 1 ? "tomorrow" : $"in {days} days";

            foreach (var a in db.Assignments.Where(a => a.Status != AssignmentStatus.Done))
            {
                if (DateOnly.FromDateTime(a.DueDate) == targetDate)
                {
                    yield return new ReminderCandidate(a.UserId, NotificationType.AssignmentDue, a.Id,
                        $"Assignment due {dayWord}", $"\"{a.Title}\" ({a.Subject}) is due {dayWord}.");
                }
            }

            foreach (var e in db.Exams)
            {
                if (DateOnly.FromDateTime(e.ExamDate) == targetDate)
                {
                    yield return new ReminderCandidate(e.UserId, NotificationType.ExamSoon, e.Id,
                        $"Exam {dayWord}", $"\"{e.Title}\" ({e.Subject}) is {dayWord}.");
                }
            }

            foreach (var s in db.Scholarships.Where(s => s.Status != ScholarshipStatus.Awarded && s.Status != ScholarshipStatus.Rejected))
            {
                if (s.Deadline == targetDate)
                {
                    yield return new ReminderCandidate(s.UserId, NotificationType.ScholarshipDeadline, s.Id,
                        $"Scholarship deadline {dayWord}", $"\"{s.Name}\" closes {dayWord}.");
                }
            }

            foreach (var c in db.CollegeApplications.Where(c => c.Status != CollegeApplicationStatus.Accepted && c.Status != CollegeApplicationStatus.Rejected))
            {
                if (c.Deadline == targetDate)
                {
                    yield return new ReminderCandidate(c.UserId, NotificationType.CollegeDeadline, c.Id,
                        $"College application deadline {dayWord}", $"Your application to \"{c.CollegeName}\" is due {dayWord}.");
                }
            }
        }
    }

    private record ReminderCandidate(Guid UserId, NotificationType Type, Guid RelatedEntityId, string Title, string Body);
}
