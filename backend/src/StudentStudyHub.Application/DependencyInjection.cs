using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using StudentStudyHub.Application.Assignments;
using StudentStudyHub.Application.Auth;
using StudentStudyHub.Application.Calendar;
using StudentStudyHub.Application.CollegeApplications;
using StudentStudyHub.Application.Courses;
using StudentStudyHub.Application.Exams;
using StudentStudyHub.Application.Notifications;
using StudentStudyHub.Application.SatPlanning;
using StudentStudyHub.Application.Scholarships;
using StudentStudyHub.Application.StudyGroups;

namespace StudentStudyHub.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<RegisterRequestValidator>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IAssignmentService, AssignmentService>();
        services.AddScoped<ICourseService, CourseService>();
        services.AddScoped<IExamService, ExamService>();
        services.AddScoped<ISatPlanService, SatPlanService>();
        services.AddScoped<IStudyGroupService, StudyGroupService>();
        services.AddScoped<IGroupNoteService, GroupNoteService>();
        services.AddScoped<IScholarshipService, ScholarshipService>();
        services.AddScoped<ICollegeApplicationService, CollegeApplicationService>();
        services.AddScoped<ICalendarService, CalendarService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IDeadlineReminderService, DeadlineReminderService>();

        return services;
    }
}
