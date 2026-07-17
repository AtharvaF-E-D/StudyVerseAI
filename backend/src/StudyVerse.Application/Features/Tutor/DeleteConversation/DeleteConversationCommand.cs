using MediatR;
using StudyVerse.Domain.Common;

namespace StudyVerse.Application.Features.Tutor.DeleteConversation;

public sealed record DeleteConversationCommand(Guid UserId, Guid ConversationId) : IRequest<Result>;
