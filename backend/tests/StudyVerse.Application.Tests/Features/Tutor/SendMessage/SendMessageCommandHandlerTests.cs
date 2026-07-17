using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using StudyVerse.Application.Common.Interfaces;
using StudyVerse.Application.Common.Models;
using StudyVerse.Application.Features.Tutor.SendMessage;
using StudyVerse.Application.Tests.TestSupport;
using StudyVerse.Domain.Common;
using StudyVerse.Domain.Entities;
using StudyVerse.Domain.Enums;

namespace StudyVerse.Application.Tests.Features.Tutor.SendMessage;

public sealed class SendMessageCommandHandlerTests
{
    private const int DailyTokenLimit = 1000;

    private readonly TestAppDbContext _db = TestDbContextFactory.Create();
    private readonly TestDateTimeProvider _dateTimeProvider = new();
    private readonly IAiChatProvider _aiChatProvider = Substitute.For<IAiChatProvider>();

    private SendMessageCommandHandler CreateHandler() => new(
        _db,
        _dateTimeProvider,
        _aiChatProvider,
        Options.Create(new AiOptions { Model = "gpt-4o-mini", DailyTokenLimit = DailyTokenLimit }),
        Substitute.For<ILogger<SendMessageCommandHandler>>());

    private Guid SeedUserWithConversation(out Guid conversationId)
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
            CreatedAtUtc = _dateTimeProvider.UtcNow,
            UpdatedAtUtc = _dateTimeProvider.UtcNow,
        };
        _db.Conversations.Add(conversation);
        _db.SaveChanges();

        conversationId = conversation.Id;
        return userId;
    }

    [Fact]
    public async Task Handle_FirstMessageInConversation_PersistsBothMessagesSetsTitleAndIncrementsUsage()
    {
        var userId = SeedUserWithConversation(out var conversationId);

        _aiChatProvider
            .GetCompletionAsync(Arg.Any<IReadOnlyList<AiChatMessage>>(), Arg.Any<CancellationToken>())
            .Returns(new AiChatResult("The Pythagorean theorem states $a^2 + b^2 = c^2$.", 100, 50));

        _aiChatProvider
            .GetSuggestedFollowUpsAsync(Arg.Any<IReadOnlyList<AiChatMessage>>(), Arg.Any<CancellationToken>())
            .Returns(new List<string> { "What is a right triangle?", "Can you give an example?" });

        var command = new SendMessageCommand(userId, conversationId, "What is the Pythagorean theorem?");
        var result = await CreateHandler().Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.UserMessage.Content.Should().Be("What is the Pythagorean theorem?");
        result.Value.UserMessage.Role.Should().Be(MessageRole.User);
        result.Value.AssistantMessage.Content.Should().Be("The Pythagorean theorem states $a^2 + b^2 = c^2$.");
        result.Value.AssistantMessage.Role.Should().Be(MessageRole.Assistant);
        result.Value.SuggestedFollowUps.Should().BeEquivalentTo(
            "What is a right triangle?", "Can you give an example?");
        result.Value.TokensUsedToday.Should().Be(150);
        result.Value.DailyLimit.Should().Be(DailyTokenLimit);

        var messages = _db.Messages.Where(m => m.ConversationId == conversationId).OrderBy(m => m.CreatedAtUtc).ToList();
        messages.Should().HaveCount(2);
        messages[0].Role.Should().Be(MessageRole.User);
        messages[1].Role.Should().Be(MessageRole.Assistant);
        messages[1].PromptTokens.Should().Be(100);
        messages[1].CompletionTokens.Should().Be(50);

        var conversation = _db.Conversations.Single(c => c.Id == conversationId);
        conversation.Title.Should().Be("What is the Pythagorean theorem?");
        conversation.UpdatedAtUtc.Should().Be(_dateTimeProvider.UtcNow);

        var progress = _db.UserProgresses.Single(p => p.UserId == userId);
        progress.AiTokensUsedToday.Should().Be(150);
        progress.AiUsageResetDateUtc.Should().Be(DateOnly.FromDateTime(_dateTimeProvider.UtcNow));
    }

    [Fact]
    public async Task Handle_WhenDailyTokenCapAlreadyReached_FailsWithRateLimitedAndNeverCallsTheAiProvider()
    {
        var userId = SeedUserWithConversation(out var conversationId);

        _db.UserProgresses.Add(new UserProgress
        {
            UserId = userId,
            AiTokensUsedToday = DailyTokenLimit,
            AiUsageResetDateUtc = DateOnly.FromDateTime(_dateTimeProvider.UtcNow),
        });
        await _db.SaveChangesAsync();

        var command = new SendMessageCommand(userId, conversationId, "Another question");
        var result = await CreateHandler().Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.ErrorType.Should().Be(ResultErrorType.RateLimited);

        await _aiChatProvider.DidNotReceiveWithAnyArgs().GetCompletionAsync(default!, default);
        (await _db.Messages.CountAsync(m => m.ConversationId == conversationId)).Should().Be(0);
    }

    [Fact]
    public async Task Handle_WhenConversationBelongsToAnotherUser_FailsWithNotFound()
    {
        SeedUserWithConversation(out var conversationId);
        var otherUserId = Guid.NewGuid();

        var command = new SendMessageCommand(otherUserId, conversationId, "Trying to hijack this conversation");
        var result = await CreateHandler().Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.ErrorType.Should().Be(ResultErrorType.NotFound);

        await _aiChatProvider.DidNotReceiveWithAnyArgs().GetCompletionAsync(default!, default);
    }
}
