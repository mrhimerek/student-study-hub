using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace StudentStudyHub.Infrastructure.Persistence;

/// <summary>
/// Used only by the `dotnet ef` CLI tools at design time (e.g. `dotnet ef migrations add`).
/// Azure Functions' local.settings.json isn't readable outside the Functions host, so migrations
/// are generated against a local LocalDB instance; at runtime the real connection string comes
/// from configuration (see Infrastructure.DependencyInjection.AddInfrastructure).
/// </summary>
public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("STUDENTSTUDYHUB_MIGRATIONS_CONNECTION")
            ?? "Server=(localdb)\\mssqllocaldb;Database=StudentStudyHub;Trusted_Connection=True;";

        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder.UseSqlServer(connectionString);

        return new ApplicationDbContext(optionsBuilder.Options);
    }
}
