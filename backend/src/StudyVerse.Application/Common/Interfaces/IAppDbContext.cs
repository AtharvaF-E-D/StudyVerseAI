using Microsoft.EntityFrameworkCore;
using StudyVerse.Domain.Entities;

namespace StudyVerse.Application.Common.Interfaces;

/// <summary>
/// Abstraction over the EF Core <c>DbContext</c> so the Application layer can depend on
/// persistence without referencing Npgsql or any other provider-specific package.
/// </summary>
public interface IAppDbContext
{
    DbSet<User> Users { get; }

    DbSet<RefreshToken> RefreshTokens { get; }

    DbSet<OtpCode> OtpCodes { get; }

    DbSet<UserToken> UserTokens { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
