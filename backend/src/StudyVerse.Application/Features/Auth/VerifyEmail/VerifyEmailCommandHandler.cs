using MediatR;
using Microsoft.EntityFrameworkCore;
using StudyVerse.Application.Common.Interfaces;
using StudyVerse.Application.Common.Security;
using StudyVerse.Domain.Common;
using StudyVerse.Domain.Enums;

namespace StudyVerse.Application.Features.Auth.VerifyEmail;

public sealed class VerifyEmailCommandHandler : IRequestHandler<VerifyEmailCommand, Result>
{
    private readonly IAppDbContext _db;
    private readonly IDateTimeProvider _dateTimeProvider;

    public VerifyEmailCommandHandler(IAppDbContext db, IDateTimeProvider dateTimeProvider)
    {
        _db = db;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<Result> Handle(VerifyEmailCommand request, CancellationToken cancellationToken)
    {
        var now = _dateTimeProvider.UtcNow;
        var tokenHash = Sha256Hasher.Hash(request.Token);

        var token = await _db.UserTokens.FirstOrDefaultAsync(
            t => t.UserId == request.UserId
                 && t.Purpose == UserTokenPurpose.EmailVerification
                 && t.TokenHash == tokenHash,
            cancellationToken);

        if (token is null || !token.IsValid(now))
        {
            return Result.Failure("Invalid or expired verification token.", ResultErrorType.Validation);
        }

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);
        if (user is null)
        {
            return Result.Failure("Invalid or expired verification token.", ResultErrorType.Validation);
        }

        token.ConsumedAtUtc = now;
        user.EmailVerified = true;
        user.UpdatedAtUtc = now;

        await _db.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
