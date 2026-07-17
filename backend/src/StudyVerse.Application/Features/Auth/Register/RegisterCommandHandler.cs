using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using StudyVerse.Application.Common.Interfaces;
using StudyVerse.Application.Common.Models;
using StudyVerse.Application.Common.Security;
using StudyVerse.Domain.Common;
using StudyVerse.Domain.Entities;
using StudyVerse.Domain.Enums;

namespace StudyVerse.Application.Features.Auth.Register;

public sealed class RegisterCommandHandler : IRequestHandler<RegisterCommand, Result<RegisterResponseDto>>
{
    private readonly IAppDbContext _db;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IEmailSender _emailSender;
    private readonly AppUrlOptions _appUrlOptions;

    public RegisterCommandHandler(
        IAppDbContext db,
        IPasswordHasher passwordHasher,
        IDateTimeProvider dateTimeProvider,
        IEmailSender emailSender,
        IOptions<AppUrlOptions> appUrlOptions)
    {
        _db = db;
        _passwordHasher = passwordHasher;
        _dateTimeProvider = dateTimeProvider;
        _emailSender = emailSender;
        _appUrlOptions = appUrlOptions.Value;
    }

    public async Task<Result<RegisterResponseDto>> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();

        var emailTaken = await _db.Users.AnyAsync(u => u.Email == normalizedEmail, cancellationToken);
        if (emailTaken)
        {
            return Result.Failure<RegisterResponseDto>(
                "This email is already registered.",
                ResultErrorType.Conflict);
        }

        var now = _dateTimeProvider.UtcNow;

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = normalizedEmail,
            DisplayName = request.DisplayName.Trim(),
            EmailVerified = false,
            AuthProvider = AuthProvider.Local,
            CreatedAtUtc = now,
            UpdatedAtUtc = now,
            IsLockedOut = false,
            FailedLoginAttempts = 0,
        };
        user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);

        _db.Users.Add(user);

        var rawToken = SecureTokenGenerator.GenerateUrlSafeToken();
        var verificationToken = new UserToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TokenHash = Sha256Hasher.Hash(rawToken),
            Purpose = UserTokenPurpose.EmailVerification,
            CreatedAtUtc = now,
            ExpiresAtUtc = now.AddHours(24),
        };
        _db.UserTokens.Add(verificationToken);

        _db.Notifications.Add(new Notification
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Title = "Welcome to StudyVerse AI",
            Body = "We're glad you're here! Verify your email to get started and begin building your study streak.",
            CreatedAtUtc = now,
        });

        await _db.SaveChangesAsync(cancellationToken);

        var verificationLink = BuildLink(_appUrlOptions.EmailVerificationUrlTemplate, user.Id, rawToken, user.Email);
        await _emailSender.SendEmailVerificationAsync(user.Email, user.DisplayName, verificationLink, cancellationToken);

        return Result.Success(new RegisterResponseDto(
            user.Id,
            user.Email,
            "Registration successful. Please check your email to verify your account."));
    }

    private static string BuildLink(string template, Guid userId, string token, string email) =>
        template
            .Replace("{userId}", Uri.EscapeDataString(userId.ToString()))
            .Replace("{token}", Uri.EscapeDataString(token))
            .Replace("{email}", Uri.EscapeDataString(email));
}
