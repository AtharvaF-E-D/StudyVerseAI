using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using StudyVerse.Application.Common.Interfaces;
using StudyVerse.Application.Common.Models;
using StudyVerse.Application.Common.Security;
using StudyVerse.Application.Features.Auth.Login;
using StudyVerse.Application.Tests.TestSupport;
using StudyVerse.Domain.Common;
using StudyVerse.Domain.Entities;
using StudyVerse.Domain.Enums;
using PasswordVerificationResult = StudyVerse.Application.Common.Models.PasswordVerificationResult;

namespace StudyVerse.Application.Tests.Features.Auth.Login;

public sealed class LoginCommandHandlerTests
{
    private readonly TestAppDbContext _db = TestDbContextFactory.Create();
    private readonly IPasswordHasher _passwordHasher = Substitute.For<IPasswordHasher>();
    private readonly IJwtTokenService _jwtTokenService = Substitute.For<IJwtTokenService>();
    private readonly TestDateTimeProvider _dateTimeProvider = new();
    private readonly TestCacheService _cache = new();
    private readonly IStreakService _streakService = Substitute.For<IStreakService>();

    private User _user = null!;

    private LoginCommandHandler CreateHandler() =>
        new(_db, _passwordHasher, _jwtTokenService, _dateTimeProvider, _cache, _streakService);

    public LoginCommandHandlerTests()
    {
        _jwtTokenService.GenerateAccessToken(Arg.Any<User>())
            .Returns(_ => new AccessTokenResult("access-token", _dateTimeProvider.UtcNow.AddMinutes(15)));
        _jwtTokenService.GenerateRefreshToken()
            .Returns(_ => new RefreshTokenResult("raw-refresh-token", "hashed-refresh-token", _dateTimeProvider.UtcNow.AddDays(30)));

        SeedUser();
    }

    private void SeedUser()
    {
        _user = new User
        {
            Id = Guid.NewGuid(),
            Email = "student@example.com",
            PasswordHash = "correct-hash",
            DisplayName = "Student",
            EmailVerified = true,
            AuthProvider = AuthProvider.Local,
            CreatedAtUtc = _dateTimeProvider.UtcNow,
            UpdatedAtUtc = _dateTimeProvider.UtcNow,
        };
        _db.Users.Add(_user);
        _db.SaveChanges();
    }

    [Fact]
    public async Task Handle_WithCorrectPassword_IssuesTokensAndResetsFailedAttempts()
    {
        _passwordHasher.VerifyPassword(Arg.Any<User>(), "correct-hash", "correct-password")
            .Returns(PasswordVerificationResult.Success);

        var handler = CreateHandler();
        var command = new LoginCommand("student@example.com", "correct-password", "device-1", "Pixel 8");

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.AccessToken.Should().Be("access-token");
        result.Value.RefreshToken.Should().Be("raw-refresh-token");
        result.Value.User.Email.Should().Be("student@example.com");

        var persistedUser = await _db.Users.SingleAsync(u => u.Id == _user.Id);
        persistedUser.FailedLoginAttempts.Should().Be(0);
        persistedUser.IsLockedOut.Should().BeFalse();

        (await _db.RefreshTokens.CountAsync(rt => rt.UserId == _user.Id && rt.DeviceId == "device-1")).Should().Be(1);
    }

    [Fact]
    public async Task Handle_WithWrongPassword_ReturnsUnauthorizedAndIncrementsFailedAttempts()
    {
        _passwordHasher.VerifyPassword(Arg.Any<User>(), "correct-hash", Arg.Any<string>())
            .Returns(PasswordVerificationResult.Failed);

        var handler = CreateHandler();
        var command = new LoginCommand("student@example.com", "wrong-password", "device-1", "Pixel 8");

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.ErrorType.Should().Be(ResultErrorType.Unauthorized);

        var persistedUser = await _db.Users.SingleAsync(u => u.Id == _user.Id);
        persistedUser.FailedLoginAttempts.Should().Be(1);
        persistedUser.IsLockedOut.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_AfterFiveConsecutiveFailures_LocksTheAccount()
    {
        _passwordHasher.VerifyPassword(Arg.Any<User>(), "correct-hash", Arg.Any<string>())
            .Returns(PasswordVerificationResult.Failed);

        var command = new LoginCommand("student@example.com", "wrong-password", "device-1", "Pixel 8");

        Result<AuthSessionDto> result = null!;
        for (var attempt = 1; attempt <= LoginCommandHandler.MaxFailedAttempts; attempt++)
        {
            // A fresh handler per attempt, same as a fresh request scope in production.
            result = await CreateHandler().Handle(command, CancellationToken.None);
        }

        result.IsSuccess.Should().BeFalse();
        result.ErrorType.Should().Be(ResultErrorType.Locked);

        var persistedUser = await _db.Users.SingleAsync(u => u.Id == _user.Id);
        persistedUser.IsLockedOut.Should().BeTrue();
        persistedUser.FailedLoginAttempts.Should().Be(LoginCommandHandler.MaxFailedAttempts);

        (await _cache.ExistsAsync(CacheKeys.LoginLockoutUntil(_user.Id))).Should().BeTrue();

        // A subsequent attempt with the CORRECT password must still be rejected while locked out.
        _passwordHasher.VerifyPassword(Arg.Any<User>(), "correct-hash", "correct-password")
            .Returns(PasswordVerificationResult.Success);

        var lockedOutAttempt = await CreateHandler().Handle(
            command with { Password = "correct-password" },
            CancellationToken.None);

        lockedOutAttempt.IsSuccess.Should().BeFalse();
        lockedOutAttempt.ErrorType.Should().Be(ResultErrorType.Locked);
    }
}
