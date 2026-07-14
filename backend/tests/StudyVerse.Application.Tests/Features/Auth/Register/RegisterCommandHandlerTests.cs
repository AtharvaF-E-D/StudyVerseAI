using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NSubstitute;
using StudyVerse.Application.Common.Interfaces;
using StudyVerse.Application.Common.Models;
using StudyVerse.Application.Features.Auth.Register;
using StudyVerse.Application.Tests.TestSupport;
using StudyVerse.Domain.Common;
using StudyVerse.Domain.Entities;

namespace StudyVerse.Application.Tests.Features.Auth.Register;

public sealed class RegisterCommandHandlerTests
{
    private readonly TestAppDbContext _db = TestDbContextFactory.Create();
    private readonly IPasswordHasher _passwordHasher = Substitute.For<IPasswordHasher>();
    private readonly TestDateTimeProvider _dateTimeProvider = new();
    private readonly IEmailSender _emailSender = Substitute.For<IEmailSender>();

    private RegisterCommandHandler CreateHandler() => new(
        _db,
        _passwordHasher,
        _dateTimeProvider,
        _emailSender,
        Options.Create(new AppUrlOptions()));

    public RegisterCommandHandlerTests()
    {
        _passwordHasher.HashPassword(Arg.Any<User>(), Arg.Any<string>()).Returns("hashed-password");
    }

    [Fact]
    public async Task Handle_WithNewEmail_CreatesUnverifiedUserAndSendsVerificationEmail()
    {
        var handler = CreateHandler();
        var command = new RegisterCommand("New.User@Example.com", "Sup3rSecret!", "New User");

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Email.Should().Be("new.user@example.com");

        var persisted = await _db.Users.SingleAsync(u => u.Email == "new.user@example.com");
        persisted.EmailVerified.Should().BeFalse();
        persisted.PasswordHash.Should().Be("hashed-password");
        persisted.DisplayName.Should().Be("New User");

        var token = await _db.UserTokens.SingleAsync(t => t.UserId == persisted.Id);
        token.Purpose.Should().Be(Domain.Enums.UserTokenPurpose.EmailVerification);
        token.ConsumedAtUtc.Should().BeNull();

        await _emailSender.Received(1).SendEmailVerificationAsync(
            "new.user@example.com",
            "New User",
            Arg.Any<string>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithDuplicateEmail_ReturnsConflictAndSendsNoEmail()
    {
        _db.Users.Add(new User
        {
            Id = Guid.NewGuid(),
            Email = "existing@example.com",
            DisplayName = "Existing User",
            AuthProvider = Domain.Enums.AuthProvider.Local,
            CreatedAtUtc = _dateTimeProvider.UtcNow,
            UpdatedAtUtc = _dateTimeProvider.UtcNow,
        });
        await _db.SaveChangesAsync();

        var handler = CreateHandler();
        var command = new RegisterCommand("Existing@Example.com", "Sup3rSecret!", "Another Name");

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.ErrorType.Should().Be(ResultErrorType.Conflict);

        (await _db.Users.CountAsync()).Should().Be(1);

        await _emailSender.DidNotReceiveWithAnyArgs().SendEmailVerificationAsync(
            default!,
            default!,
            default!,
            default);
    }
}
