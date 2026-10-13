using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using StudentStudyHub.Application.Common.Interfaces;

namespace StudentStudyHub.Functions.Timers;

/// <summary>
/// Runs hourly on the Function App's Consumption plan (free grant) to scan
/// for assignments/exams/scholarships/college deadlines within the next 3
/// days and 1 day, writing an in-app Notification and sending a reminder
/// email (via IEmailService) for each one not already notified.
/// </summary>
public class DeadlineReminderFunction(IAppDbContext db, IEmailService emailService, ILogger<DeadlineReminderFunction> logger)
{
    [Function("DeadlineReminderFunction")]
    public async Task Run([TimerTrigger("0 0 * * * *")] TimerInfo timer)
    {
        logger.LogInformation("Deadline reminder scan started at {Time}", DateTime.UtcNow);

        // TODO: query Assignments/Exams/Scholarships/CollegeApplications due in
        // 3 days or 1 day, skip ones already notified (track via Notification
        // table + RelatedEntityId), write Notification rows, and call
        // emailService.SendEmailAsync(...) for each affected user.

        await db.SaveChangesAsync();

        logger.LogInformation("Deadline reminder scan completed at {Time}", DateTime.UtcNow);
    }
}
