using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using StudyVerse.Application.Features.Tutor.ToggleBookmark;
using StudyVerse.Application.Tests.TestSupport;
using StudyVerse.Domain.Common;
using StudyVerse.Domain.Entities;
using StudyVerse.Domain.Enums;

namespace StudyVerse.Application.Tests.Features.Tutor.ToggleBookmark;

public sealed class ToggleBookmarkCommandHandlerTests
{
    private readonly TestAppDbContext _db = TestDbContextFactory.Create();
    private readonly TestDateTimeProvider _dateTimeProvider = new();

    private ToggleBookmarkCommandHandler CreateHandler() => new(_db);

    private Guid SeedUserWithConversation(out Guid conversationId, bool isBookmarked = false)
    {
        var userId = Guid.NewGuid();
        _db.Users.Add(new User
        {
            Id = userId,
            Email = $"{Guid.NewGuid():N}@example.com",
            DisplayName = "Student",
            AuthProvider = AuthProvider.Local,
            CreatedAtUtc = _dateTimeProvider.UtcNow,
            UpdatedAtUtc = _dateTimeProvider.UtcNow,
        });

        var conversation = new Conversation
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Title = "New conversation",
            IsBookmarked = isBookmarked,
            CreatedAtUtc = _dateTimeProvider.UtcNow,
            UpdatedAtUtc = _dateTimeProvider.UtcNow,
        };
        _db.Conversations.Add(conversation);
        _db.SaveChanges();

        conversationId = conversation.Id;
        return userId;
    }

    [Fact]
    public async Task Handle_OnAnUnbookmarkedConversation_SetsItToBookmarkedAndReturnsTrue()
    {
        var userId = SeedUserWithConversation(out var conversationId);

        var result = await CreateHandler().Handle(new ToggleBookmarkCommand(userId, conversationId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeTrue();

        (await _db.Conversations.SingleAsync(c => c.Id == conversationId)).IsBookmarked.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_CalledTwice_TogglesBackToUnbookmarked()
    {
        var userId = SeedUserWithConversation(out var conversationId);
        var handler = CreateHandler();

        await handler.Handle(new ToggleBookmarkCommand(userId, conversationId), CancellationToken.None);
        var second = await handler.Handle(new ToggleBookmarkCommand(userId, conversationId), CancellationToken.None);

        second.Value.Should().BeFalse();
        (await _db.Conversations.SingleAsync(c => c.Id == conversationId)).IsBookmarked.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_WhenConversationBelongsToAnotherUser_FailsWithNotFound()
    {
        SeedUserWithConversation(out var conversationId);
        var otherUserId = Guid.NewGuid();

        var result = await CreateHandler().Handle(new ToggleBookmarkCommand(otherUserId, conversationId), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.ErrorType.Should().Be(ResultErrorType.NotFound);
    }

    [Fact]
    public async Task Handle_DoesNotChangeUpdatedAtUtc()
    {
        var userId = SeedUserWithConversation(out var conversationId);
        var originalUpdatedAt = (await _db.Conversations.SingleAsync(c => c.Id == conversationId)).UpdatedAtUtc;

        _dateTimeProvider.UtcNow = _dateTimeProvider.UtcNow.AddDays(1);
        await CreateHandler().Handle(new ToggleBookmarkCommand(userId, conversationId), CancellationToken.None);

        (await _db.Conversations.SingleAsync(c => c.Id == conversationId)).UpdatedAtUtc.Should().Be(originalUpdatedAt);
    }
}
