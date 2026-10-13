using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using StudentStudyHub.Application.Assignments;
using StudentStudyHub.Application.Auth;

namespace StudentStudyHub.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<RegisterRequestValidator>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IAssignmentService, AssignmentService>();

        return services;
    }
}
