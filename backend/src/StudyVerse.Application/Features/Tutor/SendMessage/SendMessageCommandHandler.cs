using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StudyVerse.Application.Common.Interfaces;
using StudyVerse.Application.Common.Models;
using StudyVerse.Application.Features.Tutor.Common;
using StudyVerse.Domain.Common;
using StudyVerse.Domain.Entities;
using StudyVerse.Domain.Enums;

namespace StudyVerse.Application.Features.Tutor.SendMessage;

/// <summary>
/// The core AI tutor loop: validates ownership, enforces the daily token cap, persists the user's
/// message, calls the AI provider for a reply (+ a best-effort follow-up-questions call), and
/// persists everything in a single <c>SaveChangesAsync</c> at the end. That "compute everything in
/// memory, commit once" shape means that if the AI call itself throws, nothing is left half-saved
/// — the user's message is discarded along with everything else, and the caller can safely retry.
/// </summary>
public sealed class SendMessageCommandHandler : IRequestHandler<SendMessageCommand, Result<SendMessageResultDto>>
{
    private readonly IAppDbContext _db;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IAiChatProvider _aiChatProvider;
    private readonly AiOptions _aiOptions;
    private readonly ILogger<SendMessageCommandHandler> _logger;

    public SendMessageCommandHandler(
        IAppDbContext db,
        IDateTimeProvider dateTimeProvider,
        IAiChatProvider aiChatProvider,
        IOptions<AiOptions> aiOptions,
        ILogger<SendMessageCommandHandler> logger)
    {
        _db = db;
        _dateTimeProvider = dateTimeProvider;
        _aiChatProvider = aiChatProvider;
        _aiOptions = aiOptions.Value;
        _logger = logger;
    }

    public async Task<Result<SendMessageResultDto>> Handle(SendMessageCommand request, CancellationToken cancellationToken)
    {
        var conversation = await _db.Conversations.FirstOrDefaultAsync(
            c => c.Id == request.ConversationId && c.UserId == request.UserId,
            cancellationToken);

        if (conversation is null)
        {
            return Result.Failure<SendMessageResultDto>("Conversation not found.", ResultErrorType.NotFound);
        }

        var today = DateOnly.FromDateTime(_dateTimeProvider.UtcNow);

        var progress = await _db.UserProgresses.FirstOrDefaultAsync(p => p.UserId == request.UserId, cancellationToken);
        if (progress is null)
        {
            progress = new UserProgress { UserId = request.UserId };
            _db.UserProgresses.Add(progress);
        }

        AiUsagePolicy.ResetIfNewDay(progress, today);

        var dailyLimit = _aiOptions.DailyTokenLimit;
        if (progress.AiTokensUsedToday >= dailyLimit)
        {
            return Result.Failure<SendMessageResultDto>(
                "You've reached today's AI tutor usage limit. Please try again tomorrow.",
                ResultErrorType.RateLimited);
        }

        // Loaded BEFORE the new user message is added, so this both (a) supplies the prior turns
        // for the completion call and (b) tells us whether this is the conversation's first
        // exchange (for the auto-title step below).
        var priorMessages = await _db.Messages
            .Where(m => m.ConversationId == conversation.Id)
            .OrderBy(m => m.CreatedAtUtc)
            .ToListAsync(cancellationToken);

        var now = _dateTimeProvider.UtcNow;
        var userMessage = new Message
        {
            Id = Guid.NewGuid(),
            ConversationId = conversation.Id,
            Role = MessageRole.User,
            Content = request.Content.Trim(),
            CreatedAtUtc = now,
        };
        _db.Messages.Add(userMessage);

        var historyForCompletion = priorMessages
            .Select(ToAiChatMessage)
            .Append(ToAiChatMessage(userMessage))
            .ToList();

        var completion = await _aiChatProvider.GetCompletionAsync(historyForCompletion, cancellationToken);

        var assistantMessage = new Message
        {
            Id = Guid.NewGuid(),
            ConversationId = conversation.Id,
            Role = MessageRole.Assistant,
            Content = completion.Content,
            PromptTokens = completion.PromptTokens,
            CompletionTokens = completion.CompletionTokens,
            CreatedAtUtc = _dateTimeProvider.UtcNow,
        };
        _db.Messages.Add(assistantMessage);

        progress.AiTokensUsedToday += completion.PromptTokens + completion.CompletionTokens;

        if (priorMessages.Count == 0)
        {
            conversation.Title = BuildTitle(request.Content);
        }

        conversation.UpdatedAtUtc = _dateTimeProvider.UtcNow;

        var suggestedFollowUps = await GetSuggestedFollowUpsSafelyAsync(
            historyForCompletion.Append(new AiChatMessage(MessageRole.Assistant, completion.Content)).ToList(),
            conversation.Id,
            cancellationToken);

        await _db.SaveChangesAsync(cancellationToken);

        var result = new SendMessageResultDto(
            new MessageDto(userMessage.Id, userMessage.Role, userMessage.Content, userMessage.CreatedAtUtc),
            new MessageDto(assistantMessage.Id, assistantMessage.Role, assistantMessage.Content, assistantMessage.CreatedAtUtc),
            suggestedFollowUps,
            progress.AiTokensUsedToday,
            dailyLimit);

        return Result.Success(result);
    }

    private async Task<IReadOnlyList<string>> GetSuggestedFollowUpsSafelyAsync(
        IReadOnlyList<AiChatMessage> history,
        Guid conversationId,
        CancellationToken cancellationToken)
    {
        try
        {
            return await _aiChatProvider.GetSuggestedFollowUpsAsync(history, cancellationToken);
        }
        catch (Exception ex)
        {
            // Follow-up suggestions are a nice-to-have: losing them shouldn't fail (or roll back)
            // an otherwise-successful tutor reply that's already been generated and billed.
            _logger.LogWarning(ex, "Failed to fetch suggested follow-ups for conversation {ConversationId}", conversationId);
            return [];
        }
    }

    private static AiChatMessage ToAiChatMessage(Message message) => new(message.Role, message.Content);

    private const int MaxTitleLength = 60;

    private static string BuildTitle(string userContent)
    {
        var trimmed = userContent.Trim();
        return trimmed.Length <= MaxTitleLength ? trimmed : trimmed[..MaxTitleLength] + "…";
    }
}
