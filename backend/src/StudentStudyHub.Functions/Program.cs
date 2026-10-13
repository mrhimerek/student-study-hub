using System.Text.Json;
using System.Text.Json.Serialization;
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
// instead of the Functions Worker's PascalCase-by-default serializer. Enums are also
// serialized as camelCase strings (e.g. "notStarted") rather than raw ints, so the frontend
// never has to hardcode the Domain.Enums numeric ordering.
builder.Services.Configure<WorkerOptions>(options =>
{
    var jsonOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web);
    jsonOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
    options.Serializer = new JsonObjectSerializer(jsonOptions);
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
