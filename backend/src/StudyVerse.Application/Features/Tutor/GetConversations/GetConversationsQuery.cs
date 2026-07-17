using MediatR;
using StudyVerse.Domain.Common;

namespace StudyVerse.Application.Features.Tutor.GetConversations;

public sealed record GetConversationsQuery(Guid UserId, string? Search, int Take) : IRequest<Result<IReadOnlyList<ConversationSummaryDto>>>;
