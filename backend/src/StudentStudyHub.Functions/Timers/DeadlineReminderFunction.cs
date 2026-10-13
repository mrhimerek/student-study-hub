using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using StudentStudyHub.Application.Notifications;

namespace StudentStudyHub.Functions.Timers;

/// <summary>
/// Runs hourly on the Function App's Consumption plan (free grant) to scan
/// for assignments/exams/scholarships/college deadlines within the next 3
/// days and 1 day, writing an in-app Notification and sending a reminder
/// email (via IEmailService) for each one not already notified.
/// </summary>
public class DeadlineReminderFunction(IDeadlineReminderService reminderService, ILogger<DeadlineReminderFunction> logger)
{
    [Function("DeadlineReminderFunction")]
    public async Task Run([TimerTrigger("0 0 * * * *")] TimerInfo timer)
    {
        logger.LogInformation("Deadline reminder scan started at {Time}", DateTime.UtcNow);

        var created = await reminderService.ScanAndNotifyAsync();

        logger.LogInformation("Deadline reminder scan completed at {Time}; created {Count} notifications", DateTime.UtcNow, created);
    }
}
