using MediatR;
using StudyVerse.Domain.Common;

namespace StudyVerse.Application.Features.Tutor.GetConversationMessages;

public sealed record GetConversationMessagesQuery(Guid UserId, Guid ConversationId) : IRequest<Result<IReadOnlyList<MessageDto>>>;
