namespace StudentStudyHub.Application.Notifications;

public interface IDeadlineReminderService
{
    /// <summary>
    /// Scans Assignments/Exams/Scholarships/CollegeApplications across all users for items due
    /// in exactly 1 or 3 days, writes an in-app Notification (and attempts a reminder email) for
    /// each one not already notified today, and returns the number of notifications created.
    /// </summary>
    Task<int> ScanAndNotifyAsync(CancellationToken cancellationToken = default);
}
