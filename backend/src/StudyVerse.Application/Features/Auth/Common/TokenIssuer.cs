using Microsoft.EntityFrameworkCore;
using StudyVerse.Application.Common.Interfaces;
using StudyVerse.Application.Common.Models;
using StudyVerse.Domain.Entities;

namespace StudyVerse.Application.Features.Auth.Common;

/// <summary>
/// Shared logic for issuing an access + refresh token pair for a user/device, reused by every
/// flow that ends in "the caller is now signed in" (login, OTP verify, social login, and the
/// initial issuance inside the refresh-rotation flow lives separately since it doesn't touch
/// <see cref="User.LastLoginAtUtc"/>).
/// </summary>
internal static class TokenIssuer
{
    public static async Task<AuthSessionDto> IssueSessionAsync(
        IAppDbContext db,
        IJwtTokenService jwtTokenService,
        IDateTimeProvider dateTimeProvider,
        User user,
        string deviceId,
        string? deviceName,
        CancellationToken cancellationToken)
    {
        var now = dateTimeProvider.UtcNow;

        // Keep at most one active refresh token per (user, device): revoke any still-active
        // sessions on this same device before minting a new one.
        var existingActiveForDevice = await db.RefreshTokens
            .Where(rt => rt.UserId == user.Id && rt.DeviceId == deviceId && rt.RevokedAtUtc == null)
            .ToListAsync(cancellationToken);

        foreach (var stale in existingActiveForDevice)
        {
            stale.RevokedAtUtc = now;
        }

        var accessToken = jwtTokenService.GenerateAccessToken(user);
        var refreshToken = jwtTokenService.GenerateRefreshToken();

        db.RefreshTokens.Add(new Domain.Entities.RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TokenHash = refreshToken.TokenHash,
            DeviceId = deviceId,
            DeviceName = deviceName,
            CreatedAtUtc = now,
            ExpiresAtUtc = refreshToken.ExpiresAtUtc,
        });

        user.LastLoginAtUtc = now;
        user.UpdatedAtUtc = now;

        await db.SaveChangesAsync(cancellationToken);

        return new AuthSessionDto(
            accessToken.Token,
            refreshToken.RawToken,
            accessToken.ExpiresAtUtc,
            new UserSummaryDto(user.Id, user.Email, user.DisplayName, user.EmailVerified));
    }
}
