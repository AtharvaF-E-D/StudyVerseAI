using MediatR;
using Microsoft.EntityFrameworkCore;
using StudyVerse.Application.Common.Interfaces;
using StudyVerse.Application.Common.Models;
using StudyVerse.Domain.Common;
using RefreshTokenEntity = StudyVerse.Domain.Entities.RefreshToken;

namespace StudyVerse.Application.Features.Auth.RefreshToken;

public sealed class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, Result<TokenPairDto>>
{
    private readonly IAppDbContext _db;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IDateTimeProvider _dateTimeProvider;

    public RefreshTokenCommandHandler(
        IAppDbContext db,
        IJwtTokenService jwtTokenService,
        IDateTimeProvider dateTimeProvider)
    {
        _db = db;
        _jwtTokenService = jwtTokenService;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<Result<TokenPairDto>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var now = _dateTimeProvider.UtcNow;
        var tokenHash = _jwtTokenService.HashToken(request.RefreshToken);

        var storedToken = await _db.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.TokenHash == tokenHash && rt.DeviceId == request.DeviceId, cancellationToken);

        if (storedToken is null)
        {
            return Result.Failure<TokenPairDto>("Invalid refresh token.", ResultErrorType.Unauthorized);
        }

        if (storedToken.IsRevoked)
        {
            // The token was already used (or explicitly revoked) once before. Presenting it again
            // means either it leaked or a rotation race occurred — either way, treat the whole
            // session as compromised and revoke every active refresh token for this user so all
            // devices are forced to re-authenticate.
            var allActiveForUser = await _db.RefreshTokens
                .Where(rt => rt.UserId == storedToken.UserId && rt.RevokedAtUtc == null)
                .ToListAsync(cancellationToken);

            foreach (var token in allActiveForUser)
            {
                token.RevokedAtUtc = now;
            }

            await _db.SaveChangesAsync(cancellationToken);

            return Result.Failure<TokenPairDto>(
                "This refresh token has already been used. All sessions have been revoked for your security — please log in again.",
                ResultErrorType.Unauthorized);
        }

        if (storedToken.IsExpired(now))
        {
            return Result.Failure<TokenPairDto>("Refresh token has expired. Please log in again.", ResultErrorType.Unauthorized);
        }

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == storedToken.UserId, cancellationToken);
        if (user is null)
        {
            return Result.Failure<TokenPairDto>("Invalid refresh token.", ResultErrorType.Unauthorized);
        }

        var newRefreshToken = _jwtTokenService.GenerateRefreshToken();
        var newAccessToken = _jwtTokenService.GenerateAccessToken(user);

        storedToken.RevokedAtUtc = now;
        storedToken.ReplacedByTokenHash = newRefreshToken.TokenHash;

        _db.RefreshTokens.Add(new RefreshTokenEntity
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TokenHash = newRefreshToken.TokenHash,
            DeviceId = request.DeviceId,
            DeviceName = storedToken.DeviceName,
            CreatedAtUtc = now,
            ExpiresAtUtc = newRefreshToken.ExpiresAtUtc,
        });

        await _db.SaveChangesAsync(cancellationToken);

        return Result.Success(new TokenPairDto(newAccessToken.Token, newRefreshToken.RawToken, newAccessToken.ExpiresAtUtc));
    }
}
