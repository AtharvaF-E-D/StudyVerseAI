using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace StudyVerse.Infrastructure.Persistence;

/// <summary>
/// Lets `dotnet ef` create an <see cref="AppDbContext"/> at design time (e.g. `migrations add`)
/// without spinning up the full Api host / DI container, and without needing Postgres reachable.
/// The connection string here is only used to generate migrations against the Npgsql provider's
/// SQL dialect — it is never used to actually connect at design time for `migrations add`.
/// </summary>
public sealed class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__Postgres")
            ?? "Host=localhost;Port=5432;Database=studyverse;Username=studyverse;Password=studyverse_dev_password";

        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseNpgsql(connectionString);

        return new AppDbContext(optionsBuilder.Options);
    }
}
