using StudentStudyHub.Application.Common.Interfaces;

namespace StudentStudyHub.Infrastructure.Services;

public class DateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow => DateTime.UtcNow;
}
