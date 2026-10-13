using Microsoft.Extensions.Logging;
using StudentStudyHub.Application.Common.Interfaces;

namespace StudentStudyHub.Infrastructure.Services;

/// <summary>
/// Placeholder IEmailService that just logs instead of actually sending mail, so the rest of
/// the Notifications feature (Notification rows, SentViaEmail flag, DeadlineReminderService) can
/// be built and tested end-to-end without an email provider. Swap this out for a real
/// implementation (Azure Communication Services, SendGrid, etc.) when one is provisioned -
/// nothing else in the app needs to change since callers only depend on IEmailService.
/// </summary>
public class LoggingEmailService(ILogger<LoggingEmailService> logger) : IEmailService
{
    public Task SendEmailAsync(string toEmail, string subject, string htmlBody, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("(stub email) To: {ToEmail} | Subject: {Subject} | Body: {Body}", toEmail, subject, htmlBody);
        return Task.CompletedTask;
    }
}
