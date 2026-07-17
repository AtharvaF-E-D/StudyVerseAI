using MediatR;
using StudyVerse.Domain.Common;

namespace StudyVerse.Application.Features.Tutor.SendMessage;

public sealed record SendMessageCommand(Guid UserId, Guid ConversationId, string Content) : IRequest<Result<SendMessageResultDto>>;

public sealed record SendMessageResultDto(
    MessageDto UserMessage,
    MessageDto AssistantMessage,
    IReadOnlyList<string> SuggestedFollowUps,
    int TokensUsedToday,
    int DailyLimit);
