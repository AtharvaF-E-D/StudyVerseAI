using Microsoft.EntityFrameworkCore;
using StudyVerse.Application.Common.Interfaces;
using StudyVerse.Domain.Entities;

namespace StudyVerse.Application.Tests.TestSupport;

/// <summary>
/// A minimal EF Core InMemory-backed <see cref="IAppDbContext"/> for exercising Application
/// handlers against real LINQ/SaveChanges semantics without depending on the Infrastructure
/// project (which owns the Npgsql-specific <c>AppDbContext</c>).
/// </summary>
public sealed class TestAppDbContext : DbContext, IAppDbContext
{
    public TestAppDbContext(DbContextOptions<TestAppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();

    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    public DbSet<OtpCode> OtpCodes => Set<OtpCode>();

    public DbSet<UserToken> UserTokens => Set<UserToken>();

    public DbSet<UserProgress> UserProgresses => Set<UserProgress>();

    public DbSet<Notification> Notifications => Set<Notification>();

    public DbSet<ChallengeCompletion> ChallengeCompletions => Set<ChallengeCompletion>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Mirror the real Infrastructure configurations' handling of get-only computed properties.
        modelBuilder.Entity<RefreshToken>().Ignore(rt => rt.IsRevoked);
        modelBuilder.Entity<OtpCode>().Ignore(o => o.IsConsumed);
        modelBuilder.Entity<UserToken>().Ignore(t => t.IsConsumed);
        modelBuilder.Entity<Notification>().Ignore(n => n.IsRead);

        modelBuilder.Entity<UserProgress>().HasKey(p => p.UserId);

        base.OnModelCreating(modelBuilder);
    }
}
