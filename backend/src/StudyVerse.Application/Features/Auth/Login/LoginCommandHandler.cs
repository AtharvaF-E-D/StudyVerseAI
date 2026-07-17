using System.Globalization;
using MediatR;
using Microsoft.EntityFrameworkCore;
using StudyVerse.Application.Common.Interfaces;
using StudyVerse.Application.Common.Models;
using StudyVerse.Application.Common.Security;
using StudyVerse.Application.Features.Auth.Common;
using StudyVerse.Domain.Common;
using PasswordVerificationResult = StudyVerse.Application.Common.Models.PasswordVerificationResult;

namespace StudyVerse.Application.Features.Auth.Login;

public sealed class LoginCommandHandler : IRequestHandler<LoginCommand, Result<AuthSessionDto>>
{
    public const int MaxFailedAttempts = 5;
    public static readonly TimeSpan LockoutDuration = TimeSpan.FromMinutes(15);

    private readonly IAppDbContext _db;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly ICacheService _cache;
    private readonly IStreakService _streakService;

    public LoginCommandHandler(
        IAppDbContext db,
        IPasswordHasher passwordHasher,
        IJwtTokenService jwtTokenService,
        IDateTimeProvider dateTimeProvider,
        ICacheService cache,
        IStreakService streakService)
    {
        _db = db;
        _passwordHasher = passwordHasher;
        _jwtTokenService = jwtTokenService;
        _dateTimeProvider = dateTimeProvider;
        _cache = cache;
        _streakService = streakService;
    }

    public async Task<Result<AuthSessionDto>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var now = _dateTimeProvider.UtcNow;

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == normalizedEmail, cancellationToken);
        if (user is null)
        {
            // Same message as a wrong password, to avoid leaking which emails are registered.
            return Result.Failure<AuthSessionDto>("Invalid email or password.", ResultErrorType.Unauthorized);
        }

        var lockoutCacheKey = CacheKeys.LoginLockoutUntil(user.Id);
        var cachedLockoutUntil = await _cache.GetAsync(lockoutCacheKey, cancellationToken);
        if (cachedLockoutUntil is not null)
        {
            var lockoutUntil = DateTime.Parse(cachedLockoutUntil, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
            if (lockoutUntil > now)
            {
                return Result.Failure<AuthSessionDto>(
                    "This account is temporarily locked due to too many failed login attempts. Please try again later.",
                    ResultErrorType.Locked);
            }

            // Lockout window has elapsed: clear stale state before continuing.
            await _cache.RemoveAsync(lockoutCacheKey, cancellationToken);
            user.IsLockedOut = false;
            user.FailedLoginAttempts = 0;
        }

        if (user.PasswordHash is null)
        {
            return Result.Failure<AuthSessionDto>(
                "This account does not have a password. Sign in with the provider you used to create it, or use 'forgot password' to set one.",
                ResultErrorType.Unauthorized);
        }

        var verification = _passwordHasher.VerifyPassword(user, user.PasswordHash, request.Password);
        if (verification == PasswordVerificationResult.Failed)
        {
            user.FailedLoginAttempts++;
            ResultErrorType errorType = ResultErrorType.Unauthorized;
            var message = "Invalid email or password.";

            if (user.FailedLoginAttempts >= MaxFailedAttempts)
            {
                user.IsLockedOut = true;
                var lockoutUntil = now.Add(LockoutDuration);
                await _cache.SetAsync(
                    lockoutCacheKey,
                    lockoutUntil.ToString("O", CultureInfo.InvariantCulture),
                    LockoutDuration,
                    cancellationToken);

                errorType = ResultErrorType.Locked;
                message = "This account is temporarily locked due to too many failed login attempts. Please try again later.";
            }

            user.UpdatedAtUtc = now;
            await _db.SaveChangesAsync(cancellationToken);

            return Result.Failure<AuthSessionDto>(message, errorType);
        }

        if (verification == PasswordVerificationResult.SuccessRehashNeeded)
        {
            user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);
        }

        user.FailedLoginAttempts = 0;
        user.IsLockedOut = false;

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
