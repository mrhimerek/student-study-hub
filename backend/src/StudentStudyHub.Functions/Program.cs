using System.Text.Json;
using Azure.Core.Serialization;
using Azure.Monitor.OpenTelemetry.Exporter;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Azure.Functions.Worker.OpenTelemetry;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetry;
using StudentStudyHub.Application;
using StudentStudyHub.Application.Common.Interfaces;
using StudentStudyHub.Functions.Auth;
using StudentStudyHub.Functions.Middleware;
using StudentStudyHub.Infrastructure;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

// Use camelCase JSON (the web/JS convention) for both request binding and response bodies,
// instead of the Functions Worker's PascalCase-by-default serializer.
builder.Services.Configure<WorkerOptions>(options =>
{
    options.Serializer = new JsonObjectSerializer(new JsonSerializerOptions(JsonSerializerDefaults.Web));
});

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// Scoped per-invocation "current user" populated by JwtAuthMiddleware from the validated JWT.
builder.Services.AddScoped<CurrentUserService>();
builder.Services.AddScoped<ICurrentUserService>(sp => sp.GetRequiredService<CurrentUserService>());

builder.UseMiddleware<JwtAuthMiddleware>();

if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("APPLICATIONINSIGHTS_CONNECTION_STRING")))
{
    builder.Services.AddOpenTelemetry()
        .UseFunctionsWorkerDefaults()
        .UseAzureMonitorExporter();
}

builder.Build().Run();
