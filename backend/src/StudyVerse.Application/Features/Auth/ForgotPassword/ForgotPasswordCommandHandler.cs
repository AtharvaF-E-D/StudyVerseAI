using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using StudyVerse.Application.Common.Interfaces;
using StudyVerse.Application.Common.Models;
using StudyVerse.Application.Common.Security;
using StudyVerse.Domain.Common;
using StudyVerse.Domain.Entities;
using StudyVerse.Domain.Enums;

namespace StudyVerse.Application.Features.Auth.ForgotPassword;

public sealed class ForgotPasswordCommandHandler : IRequestHandler<ForgotPasswordCommand, Result>
{
    private readonly IAppDbContext _db;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IEmailSender _emailSender;
    private readonly AppUrlOptions _appUrlOptions;

    public ForgotPasswordCommandHandler(
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

    public async Task<Result> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var now = _dateTimeProvider.UtcNow;

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == normalizedEmail, cancellationToken);

        // Always report success, whether or not the account exists, to avoid user enumeration.
        if (user is null)
        {
            return Result.Success();
        }

        var outstandingTokens = await _db.UserTokens
            .Where(t => t.UserId == user.Id && t.Purpose == UserTokenPurpose.PasswordReset && t.ConsumedAtUtc == null)
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
            Purpose = UserTokenPurpose.PasswordReset,
            CreatedAtUtc = now,
            ExpiresAtUtc = now.AddHours(1),
        });

        await _db.SaveChangesAsync(cancellationToken);

        var link = _appUrlOptions.PasswordResetUrlTemplate
            .Replace("{email}", Uri.EscapeDataString(user.Email))
            .Replace("{token}", Uri.EscapeDataString(rawToken));

        await _emailSender.SendPasswordResetAsync(user.Email, user.DisplayName, link, cancellationToken);

        return Result.Success();
    }
}
