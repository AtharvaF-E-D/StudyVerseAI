using MediatR;
using StudyVerse.Application.Features.CurrentAffairs.Common;
using StudyVerse.Domain.Common;

namespace StudyVerse.Application.Features.CurrentAffairs.ToggleBookmark;

/// <summary>Toggles whether <paramref name="UserId"/> has bookmarked <paramref name="ArticleId"/> -
/// bookmark it if not already, remove the bookmark if it is. See <c>NewsBookmark</c>'s doc comment
/// for why "does a row exist" is the sole source of truth for the current state.</summary>
public sealed record ToggleBookmarkCommand(Guid UserId, Guid ArticleId) : IRequest<Result<ToggleBookmarkResultDto>>;
