using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using StudentStudyHub.Application.Assignments;
using StudentStudyHub.Application.Auth;
using StudentStudyHub.Application.Courses;
using StudentStudyHub.Application.Exams;
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
        services.AddScoped<IStudyGroupService, StudyGroupService>();
        services.AddScoped<IGroupNoteService, GroupNoteService>();

        return services;
    }
}
