using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using StudyVerse.Application.Common.Interfaces;
using StudyVerse.Application.Common.Models;
using StudyVerse.Application.Common.Security;
using StudyVerse.Domain.Common;
using StudyVerse.Domain.Entities;
using StudyVerse.Domain.Enums;

namespace StudyVerse.Application.Features.Auth.ResendVerification;

public sealed class ResendVerificationCommandHandler : IRequestHandler<ResendVerificationCommand, Result>
{
    private readonly IAppDbContext _db;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IEmailSender _emailSender;
    private readonly AppUrlOptions _appUrlOptions;

    public ResendVerificationCommandHandler(
        IAppDbContext db,
        IDateTimeProvider dateTimeProvider,
        IEmailSender emailSender,
        IOptions<AppUrlOptions> appUrlOptions)
    {
        _db = db;
        _dateTimeProvider = dateTimeProvider;
        _emailSender = emailSender;
        _appUrlOptions = appUrlOptions.Value;
    }

    public async Task<Result> Handle(ResendVerificationCommand request, CancellationToken cancellationToken)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var now = _dateTimeProvider.UtcNow;

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == normalizedEmail, cancellationToken);

        // Always report success regardless of whether the account exists or is already verified,
        // so this endpoint cannot be used to enumerate registered emails.
        if (user is null || user.EmailVerified)
        {
            return Result.Success();
        }

        var outstandingTokens = await _db.UserTokens
            .Where(t => t.UserId == user.Id && t.Purpose == UserTokenPurpose.EmailVerification && t.ConsumedAtUtc == null)
            .ToListAsync(cancellationToken);

        foreach (var stale in outstandingTokens)
        {
            stale.ConsumedAtUtc = now;
        }

        var rawToken = SecureTokenGenerator.GenerateUrlSafeToken();
        _db.UserTokens.Add(new UserToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TokenHash = Sha256Hasher.Hash(rawToken),
            Purpose = UserTokenPurpose.EmailVerification,
            CreatedAtUtc = now,
            ExpiresAtUtc = now.AddHours(24),
        });

        await _db.SaveChangesAsync(cancellationToken);

        var link = _appUrlOptions.EmailVerificationUrlTemplate
            .Replace("{userId}", Uri.EscapeDataString(user.Id.ToString()))
            .Replace("{token}", Uri.EscapeDataString(rawToken))
            .Replace("{email}", Uri.EscapeDataString(user.Email));

        await _emailSender.SendEmailVerificationAsync(user.Email, user.DisplayName, link, cancellationToken);

        return Result.Success();
    }
}
