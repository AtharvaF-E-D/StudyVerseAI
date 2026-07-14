using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using StudyVerse.Application.Common.Interfaces;
using StudyVerse.Application.Common.Models;
using StudyVerse.Application.Tests.TestSupport;
using StudyVerse.Domain.Common;
using StudyVerse.Domain.Entities;
using StudyVerse.Domain.Enums;
using RefreshTokenCommand = StudyVerse.Application.Features.Auth.RefreshToken.RefreshTokenCommand;
using RefreshTokenCommandHandler = StudyVerse.Application.Features.Auth.RefreshToken.RefreshTokenCommandHandler;
using RefreshTokenEntity = StudyVerse.Domain.Entities.RefreshToken;

namespace StudyVerse.Application.Tests.Features.Auth.RefreshToken;

public sealed class RefreshTokenCommandHandlerTests
{
    private readonly TestAppDbContext _db = TestDbContextFactory.Create();
    private readonly IJwtTokenService _jwtTokenService = Substitute.For<IJwtTokenService>();
    private readonly TestDateTimeProvider _dateTimeProvider = new();

    private User _user = null!;

    public RefreshTokenCommandHandlerTests()
    {
        // Deterministic fake hash: "hash:<raw>" — lets tests seed rows whose TokenHash the
        // handler will actually look up when given the corresponding raw token.
        _jwtTokenService.HashToken(Arg.Any<string>()).Returns(ci => "hash:" + ci.Arg<string>());

        _jwtTokenService.GenerateAccessToken(Arg.Any<User>())
            .Returns(_ => new AccessTokenResult("new-access-token", _dateTimeProvider.UtcNow.AddMinutes(15)));
        _jwtTokenService.GenerateRefreshToken()
            .Returns(_ => new RefreshTokenResult("raw-new-token", "hash:raw-new-token", _dateTimeProvider.UtcNow.AddDays(30)));

        _user = new User
        {
            Id = Guid.NewGuid(),
            Email = "student@example.com",
            DisplayName = "Student",
            AuthProvider = AuthProvider.Local,
            CreatedAtUtc = _dateTimeProvider.UtcNow,
            UpdatedAtUtc = _dateTimeProvider.UtcNow,
        };
        _db.Users.Add(_user);
        _db.SaveChanges();
    }

    private RefreshTokenCommandHandler CreateHandler() => new(_db, _jwtTokenService, _dateTimeProvider);

    [Fact]
    public async Task Handle_WithValidActiveToken_RotatesAndIssuesNewTokenPair()
    {
        var oldToken = new RefreshTokenEntity
        {
            Id = Guid.NewGuid(),
            UserId = _user.Id,
            TokenHash = "hash:raw-old-token",
            DeviceId = "device-1",
            CreatedAtUtc = _dateTimeProvider.UtcNow.AddDays(-1),
            ExpiresAtUtc = _dateTimeProvider.UtcNow.AddDays(10),
        };
        _db.RefreshTokens.Add(oldToken);
        await _db.SaveChangesAsync();

        var handler = CreateHandler();
        var result = await handler.Handle(new RefreshTokenCommand("raw-old-token", "device-1"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.AccessToken.Should().Be("new-access-token");
        result.Value.RefreshToken.Should().Be("raw-new-token");

        var persistedOldToken = await _db.RefreshTokens.SingleAsync(rt => rt.Id == oldToken.Id);
        persistedOldToken.IsRevoked.Should().BeTrue();
        persistedOldToken.ReplacedByTokenHash.Should().Be("hash:raw-new-token");

        var newToken = await _db.RefreshTokens.SingleAsync(rt => rt.TokenHash == "hash:raw-new-token");
        newToken.UserId.Should().Be(_user.Id);
        newToken.DeviceId.Should().Be("device-1");
        newToken.RevokedAtUtc.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WhenAnAlreadyRevokedTokenIsReused_RevokesAllActiveSessionsForTheUser()
    {
        var reusedToken = new RefreshTokenEntity
        {
            Id = Guid.NewGuid(),
            UserId = _user.Id,
            TokenHash = "hash:stolen-token",
            DeviceId = "device-1",
            CreatedAtUtc = _dateTimeProvider.UtcNow.AddDays(-2),
            ExpiresAtUtc = _dateTimeProvider.UtcNow.AddDays(28),
            RevokedAtUtc = _dateTimeProvider.UtcNow.AddDays(-1), // already used once
        };
        var otherActiveSession = new RefreshTokenEntity
        {
            Id = Guid.NewGuid(),
            UserId = _user.Id,
            TokenHash = "hash:other-active-token",
            DeviceId = "device-2",
            CreatedAtUtc = _dateTimeProvider.UtcNow.AddDays(-1),
            ExpiresAtUtc = _dateTimeProvider.UtcNow.AddDays(29),
        };
        _db.RefreshTokens.AddRange(reusedToken, otherActiveSession);
        await _db.SaveChangesAsync();

        var handler = CreateHandler();
        var result = await handler.Handle(new RefreshTokenCommand("stolen-token", "device-1"), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.ErrorType.Should().Be(ResultErrorType.Unauthorized);

        var persistedOtherSession = await _db.RefreshTokens.SingleAsync(rt => rt.Id == otherActiveSession.Id);
        persistedOtherSession.IsRevoked.Should().BeTrue("reuse of a rotated-out token must revoke every active session for the user");

        // No new token should have been issued.
        (await _db.RefreshTokens.CountAsync()).Should().Be(2);
    }
}
