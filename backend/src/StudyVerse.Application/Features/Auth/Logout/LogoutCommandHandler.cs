using MediatR;
using Microsoft.EntityFrameworkCore;
using StudyVerse.Application.Common.Interfaces;
using StudyVerse.Domain.Common;

namespace StudyVerse.Application.Features.Auth.Logout;

public sealed class LogoutCommandHandler : IRequestHandler<LogoutCommand, Result>
{
    private readonly IAppDbContext _db;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IDateTimeProvider _dateTimeProvider;

    public LogoutCommandHandler(IAppDbContext db, IJwtTokenService jwtTokenService, IDateTimeProvider dateTimeProvider)
    {
        _db = db;
        _jwtTokenService = jwtTokenService;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<Result> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        var tokenHash = _jwtTokenService.HashToken(request.RefreshToken);

        var storedToken = await _db.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.TokenHash == tokenHash && rt.DeviceId == request.DeviceId, cancellationToken);

        // Logout is idempotent: an already-revoked or unknown token still results in success, so
        // callers can't use this endpoint to probe for the existence of a token/device pair.
        if (storedToken is not null && storedToken.RevokedAtUtc is null)
        {
            storedToken.RevokedAtUtc = _dateTimeProvider.UtcNow;
            await _db.SaveChangesAsync(cancellationToken);
        }

        return Result.Success();
    }
}
