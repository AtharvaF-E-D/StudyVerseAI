using MediatR;
using StudyVerse.Application.Common.Interfaces;
using StudyVerse.Application.Common.Security;
using StudyVerse.Domain.Common;
using StudyVerse.Domain.Entities;
using StudyVerse.Domain.Enums;

namespace StudyVerse.Application.Features.Auth.RequestOtp;

public sealed class RequestOtpCommandHandler : IRequestHandler<RequestOtpCommand, Result>
{
    private static readonly TimeSpan ThrottleWindow = TimeSpan.FromSeconds(60);
    private static readonly TimeSpan CodeLifetime = TimeSpan.FromMinutes(5);

    private readonly IAppDbContext _db;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly ICacheService _cache;
    private readonly IOtpSender _otpSender;

    public RequestOtpCommandHandler(
        IAppDbContext db,
        IDateTimeProvider dateTimeProvider,
        ICacheService cache,
        IOtpSender otpSender)
    {
        _db = db;
        _dateTimeProvider = dateTimeProvider;
        _cache = cache;
        _otpSender = otpSender;
    }

    public async Task<Result> Handle(RequestOtpCommand request, CancellationToken cancellationToken)
    {
        var destination = NormalizeDestination(request.Channel, request.Destination);
        var throttleKey = CacheKeys.OtpRequestThrottle(request.Channel, destination);

        if (await _cache.ExistsAsync(throttleKey, cancellationToken))
        {
            return Result.Failure(
                "Please wait before requesting another code.",
                ResultErrorType.RateLimited);
        }

        var now = _dateTimeProvider.UtcNow;
        var code = SecureTokenGenerator.GenerateNumericCode();

        _db.OtpCodes.Add(new OtpCode
        {
            Id = Guid.NewGuid(),
            Destination = destination,
            Channel = request.Channel,
            CodeHash = Sha256Hasher.Hash(code),
            Purpose = request.Purpose,
            CreatedAtUtc = now,
            ExpiresAtUtc = now.Add(CodeLifetime),
            AttemptCount = 0,
        });

        await _db.SaveChangesAsync(cancellationToken);

        // Set the throttle marker only after the code is durably persisted and about to be sent,
        // so a transient DB failure doesn't lock the user out of retrying immediately.
        await _cache.SetAsync(throttleKey, "1", ThrottleWindow, cancellationToken);

        await _otpSender.SendOtpAsync(request.Channel, destination, code, request.Purpose, cancellationToken);

        return Result.Success();
    }

    private static string NormalizeDestination(OtpChannel channel, string destination) =>
        channel == OtpChannel.Email ? destination.Trim().ToLowerInvariant() : destination.Trim();
}
