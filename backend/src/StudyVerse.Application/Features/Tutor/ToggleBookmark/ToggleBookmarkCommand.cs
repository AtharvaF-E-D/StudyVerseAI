using MediatR;
using StudyVerse.Domain.Common;

namespace StudyVerse.Application.Features.Tutor.ToggleBookmark;

/// <summary>Flips <c>Conversation.IsBookmarked</c> and returns the new value.</summary>
public sealed record ToggleBookmarkCommand(Guid UserId, Guid ConversationId) : IRequest<Result<bool>>;
