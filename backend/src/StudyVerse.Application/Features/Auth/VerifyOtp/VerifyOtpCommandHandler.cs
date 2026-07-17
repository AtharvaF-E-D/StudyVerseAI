using MediatR;
using Microsoft.EntityFrameworkCore;
using StudyVerse.Application.Common.Interfaces;
using StudyVerse.Application.Common.Models;
using StudyVerse.Application.Common.Security;
using StudyVerse.Application.Features.Auth.Common;
using StudyVerse.Domain.Common;
using StudyVerse.Domain.Entities;
using StudyVerse.Domain.Enums;

namespace StudyVerse.Application.Features.Auth.VerifyOtp;

/// <summary>
/// Verifies an OTP code and, on success, always issues an access/refresh token pair — the
/// <c>POST /otp/verify</c> endpoint contract returns the same shape as login regardless of
/// <see cref="OtpPurpose"/>. Non-obvious design decisions:
///
/// 1. Purpose=Login auto-provisions a brand-new account on first-time OTP login, but ONLY for
///    the Email channel: <see cref="User.Email"/> is a required, unique domain field, and a bare
///    phone number cannot satisfy it. A phone-channel login OTP for an unrecognized number
///    therefore fails with "no account linked" rather than fabricating a placeholder email —
///    phone numbers must first be linked to an existing (email-registered) account.
/// 2. Purpose=EmailVerification / PasswordReset require an existing account. Verifying the code
///    marks the email verified (for EmailVerification) and, either way, proves the caller
///    controls that destination — which is treated as equivalent to a login, so a session is
///    issued. The actual password change for a PasswordReset-purpose OTP still goes through
///    <c>POST /reset-password</c> with a token — OTP verification here only authenticates the
///    user so the client can complete that follow-up step while signed in.
/// </summary>
public sealed class VerifyOtpCommandHandler : IRequestHandler<VerifyOtpCommand, Result<AuthSessionDto>>
{
    public const int MaxAttempts = 5;

    private readonly IAppDbContext _db;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IStreakService _streakService;

    public VerifyOtpCommandHandler(
        IAppDbContext db,
        IJwtTokenService jwtTokenService,
        IDateTimeProvider dateTimeProvider,
        IStreakService streakService)
    {
        _db = db;
        _jwtTokenService = jwtTokenService;
        _dateTimeProvider = dateTimeProvider;
        _streakService = streakService;
    }

    public async Task<Result<AuthSessionDto>> Handle(VerifyOtpCommand request, CancellationToken cancellationToken)
    {
        var destination = request.Channel == OtpChannel.Email
            ? request.Destination.Trim().ToLowerInvariant()
            : request.Destination.Trim();

        var now = _dateTimeProvider.UtcNow;

        var otp = await _db.OtpCodes
            .Where(o => o.Destination == destination
                        && o.Channel == request.Channel
                        && o.Purpose == request.Purpose
                        && o.ConsumedAtUtc == null)
            .OrderByDescending(o => o.CreatedAtUtc)
            .FirstOrDefaultAsync(cancellationToken);

        if (otp is null || otp.IsExpired(now))
        {
            return Result.Failure<AuthSessionDto>("Invalid or expired code.", ResultErrorType.Validation);
        }

        if (otp.AttemptCount >= MaxAttempts)
        {
            otp.ConsumedAtUtc = now;
            await _db.SaveChangesAsync(cancellationToken);
            return Result.Failure<AuthSessionDto>("Too many attempts. Please request a new code.", ResultErrorType.Validation);
        }

        var providedHash = Sha256Hasher.Hash(request.Code);
        if (!string.Equals(providedHash, otp.CodeHash, StringComparison.Ordinal))
        {
            otp.AttemptCount++;
            if (otp.AttemptCount >= MaxAttempts)
            {
                otp.ConsumedAtUtc = now;
            }

            await _db.SaveChangesAsync(cancellationToken);
            return Result.Failure<AuthSessionDto>("Invalid code.", ResultErrorType.Validation);
        }

        otp.ConsumedAtUtc = now;

        var user = request.Channel == OtpChannel.Email
            ? await _db.Users.FirstOrDefaultAsync(u => u.Email == destination, cancellationToken)
            : await _db.Users.FirstOrDefaultAsync(u => u.PhoneNumber == destination, cancellationToken);

        if (request.Purpose == OtpPurpose.Login)
        {
            if (user is null)
            {
                if (request.Channel != OtpChannel.Email)
                {
                    return Result.Failure<AuthSessionDto>(
                        "No account is linked to this phone number. Please register with email first.",
                        ResultErrorType.NotFound);
                }

                user = new User
                {
                    Id = Guid.NewGuid(),
                    Email = destination,
                    DisplayName = destination[..destination.IndexOf('@')],
                    EmailVerified = true,
                    AuthProvider = AuthProvider.Local,
                    CreatedAtUtc = now,
                    UpdatedAtUtc = now,
                    IsLockedOut = false,
                    FailedLoginAttempts = 0,
                };
                _db.Users.Add(user);
            }
        }
        else
        {
            if (user is null)
            {
                return Result.Failure<AuthSessionDto>("No account found for this destination.", ResultErrorType.NotFound);
            }

            if (request.Purpose == OtpPurpose.EmailVerification)
            {
                user.EmailVerified = true;
                user.UpdatedAtUtc = now;
            }
        }

        await _db.SaveChangesAsync(cancellationToken);

        if (request.Purpose == OtpPurpose.Login)
        {
            await _streakService.RecordActivityAsync(user.Id, cancellationToken);
        }

        var session = await TokenIssuer.IssueSessionAsync(
            _db,
            _jwtTokenService,
            _dateTimeProvider,
            user,
            request.DeviceId,
            request.DeviceName,
            cancellationToken);

        return Result.Success(session);
    }
}
