using MediatR;
using Microsoft.EntityFrameworkCore;
using StudyVerse.Application.Common.Interfaces;
using StudyVerse.Application.Common.Security;
using StudyVerse.Domain.Common;
using StudyVerse.Domain.Enums;

namespace StudyVerse.Application.Features.Auth.ResetPassword;

public sealed class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, Result>
{
    private readonly IAppDbContext _db;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IDateTimeProvider _dateTimeProvider;

    public ResetPasswordCommandHandler(IAppDbContext db, IPasswordHasher passwordHasher, IDateTimeProvider dateTimeProvider)
    {
        _db = db;
        _passwordHasher = passwordHasher;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<Result> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var now = _dateTimeProvider.UtcNow;

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == normalizedEmail, cancellationToken);
        if (user is null)
        {
            return Result.Failure("Invalid or expired reset token.", ResultErrorType.Validation);
        }

        var tokenHash = Sha256Hasher.Hash(request.Token);
        var token = await _db.UserTokens.FirstOrDefaultAsync(
            t => t.UserId == user.Id && t.Purpose == UserTokenPurpose.PasswordReset && t.TokenHash == tokenHash,
            cancellationToken);

        if (token is null || !token.IsValid(now))
        {
            return Result.Failure("Invalid or expired reset token.", ResultErrorType.Validation);
        }

        token.ConsumedAtUtc = now;
        user.PasswordHash = _passwordHasher.HashPassword(user, request.NewPassword);
        user.UpdatedAtUtc = now;
        user.FailedLoginAttempts = 0;
        user.IsLockedOut = false;

        var activeRefreshTokens = await _db.RefreshTokens
            .Where(rt => rt.UserId == user.Id && rt.RevokedAtUtc == null)
            .ToListAsync(cancellationToken);

        foreach (var refreshToken in activeRefreshTokens)
        {
            refreshToken.RevokedAtUtc = now;
        }

        await _db.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
