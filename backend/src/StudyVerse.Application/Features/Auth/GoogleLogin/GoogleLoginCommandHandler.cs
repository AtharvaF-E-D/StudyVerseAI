using MediatR;
using Microsoft.EntityFrameworkCore;
using StudyVerse.Application.Common.Interfaces;
using StudyVerse.Application.Common.Models;
using StudyVerse.Application.Features.Auth.Common;
using StudyVerse.Domain.Common;
using StudyVerse.Domain.Entities;
using StudyVerse.Domain.Enums;

namespace StudyVerse.Application.Features.Auth.GoogleLogin;

public sealed class GoogleLoginCommandHandler : IRequestHandler<GoogleLoginCommand, Result<AuthSessionDto>>
{
    private readonly IAppDbContext _db;
    private readonly IGoogleTokenValidator _googleTokenValidator;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IStreakService _streakService;

    public GoogleLoginCommandHandler(
        IAppDbContext db,
        IGoogleTokenValidator googleTokenValidator,
        IJwtTokenService jwtTokenService,
        IDateTimeProvider dateTimeProvider,
        IStreakService streakService)
    {
        _db = db;
        _googleTokenValidator = googleTokenValidator;
        _jwtTokenService = jwtTokenService;
        _dateTimeProvider = dateTimeProvider;
        _streakService = streakService;
    }

    public async Task<Result<AuthSessionDto>> Handle(GoogleLoginCommand request, CancellationToken cancellationToken)
    {
        var externalUser = await _googleTokenValidator.ValidateAsync(request.IdToken, cancellationToken);
        if (externalUser is null)
        {
            return Result.Failure<AuthSessionDto>("Invalid or expired Google ID token.", ResultErrorType.Unauthorized);
        }

        var normalizedEmail = externalUser.Email.Trim().ToLowerInvariant();
        var now = _dateTimeProvider.UtcNow;

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == normalizedEmail, cancellationToken);
        if (user is null)
        {
            user = new User
            {
                Id = Guid.NewGuid(),
                Email = normalizedEmail,
                DisplayName = string.IsNullOrWhiteSpace(externalUser.DisplayName)
                    ? normalizedEmail[..normalizedEmail.IndexOf('@')]
                    : externalUser.DisplayName,
                EmailVerified = externalUser.EmailVerified,
                AuthProvider = AuthProvider.Google,
                CreatedAtUtc = now,
                UpdatedAtUtc = now,
                IsLockedOut = false,
                FailedLoginAttempts = 0,
            };
            _db.Users.Add(user);
            await _db.SaveChangesAsync(cancellationToken);
        }
        else if (externalUser.EmailVerified && !user.EmailVerified)
        {
            // Google has vouched for this email; trust it even if the account was originally
            // created locally and never completed our own email-verification flow.
            user.EmailVerified = true;
            user.UpdatedAtUtc = now;
            await _db.SaveChangesAsync(cancellationToken);
        }

        await _streakService.RecordActivityAsync(user.Id, cancellationToken);

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
