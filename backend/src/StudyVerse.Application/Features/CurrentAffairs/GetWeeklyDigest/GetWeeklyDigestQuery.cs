using MediatR;
using StudyVerse.Application.Features.CurrentAffairs.Common;
using StudyVerse.Domain.Common;

namespace StudyVerse.Application.Features.CurrentAffairs.GetWeeklyDigest;

/// <summary>
/// Cache-first and shared across ALL users - see <c>WeeklyDigest</c>'s doc comment. Built from
/// whatever's already cached in the DB for the current ISO week (no fresh GNews calls triggered just
/// for this). Fails with <see cref="ResultErrorType.NotFound"/> - not a fabricated summary - when
/// nobody has browsed any category yet this week and there's nothing to summarize.
/// </summary>
public sealed record GetWeeklyDigestQuery : IRequest<Result<WeeklyDigestDto>>;
