using MediatR;
using StudyVerse.Application.Features.CodingPractice.Common;
using StudyVerse.Domain.Common;

namespace StudyVerse.Application.Features.CodingPractice.GetDailyCodingChallenge;

/// <summary>Today's rotated problem - the same for every user on a given UTC calendar day; see
/// <c>DailyCodingChallengeSelector</c>. No <c>UserId</c>: unlike <c>GetProblemsQuery</c>'s
/// per-problem <c>isSolved</c>, this endpoint is purely "what is today's challenge".</summary>
public sealed record GetDailyCodingChallengeQuery : IRequest<Result<DailyCodingChallengeDto>>;
