using MediatR;
using Microsoft.EntityFrameworkCore;
using StudyVerse.Application.Common.Interfaces;
using StudyVerse.Application.Common.Models;
using StudyVerse.Application.Features.Auth.Common;
using StudyVerse.Domain.Common;
using StudyVerse.Domain.Entities;
using StudyVerse.Domain.Enums;

namespace StudyVerse.Application.Features.Auth.AppleLogin;

public sealed class AppleLoginCommandHandler : IRequestHandler<AppleLoginCommand, Result<AuthSessionDto>>
{
    private readonly IAppDbContext _db;
    private readonly IAppleTokenValidator _appleTokenValidator;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IDateTimeProvider _dateTimeProvider;

    public AppleLoginCommandHandler(
        IAppDbContext db,
        IAppleTokenValidator appleTokenValidator,
        IJwtTokenService jwtTokenService,
        IDateTimeProvider dateTimeProvider)
    {
        _db = db;
        _appleTokenValidator = appleTokenValidator;
        _jwtTokenService = jwtTokenService;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<Result<AuthSessionDto>> Handle(AppleLoginCommand request, CancellationToken cancellationToken)
    {
        var externalUser = await _appleTokenValidator.ValidateAsync(request.IdentityToken, cancellationToken);
        if (externalUser is null)
        {
            return Result.Failure<AuthSessionDto>("Invalid or expired Apple identity token.", ResultErrorType.Unauthorized);
        }

        var normalizedEmail = externalUser.Email.Trim().ToLowerInvariant();
        var now = _dateTimeProvider.UtcNow;

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == normalizedEmail, cancellationToken);
        if (user is null)
        {
            // Apple only ever sends the user's full name on the very first authorization, and it
            // comes from the client (not the identity token itself), hence it's a separate
            // command parameter rather than something parsed off externalUser.
            var displayName = !string.IsNullOrWhiteSpace(request.FullName)
                ? request.FullName!.Trim()
                : normalizedEmail[..normalizedEmail.IndexOf('@')];

            user = new User
            {
                Id = Guid.NewGuid(),
                Email = normalizedEmail,
                DisplayName = displayName,
                EmailVerified = externalUser.EmailVerified,
                AuthProvider = AuthProvider.Apple,
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
            user.EmailVerified = true;
            user.UpdatedAtUtc = now;
            await _db.SaveChangesAsync(cancellationToken);
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
