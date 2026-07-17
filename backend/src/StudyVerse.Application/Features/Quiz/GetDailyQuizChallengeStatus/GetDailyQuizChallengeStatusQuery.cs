using MediatR;
using StudyVerse.Domain.Common;
using StudyVerse.Domain.Enums;

namespace StudyVerse.Application.Features.Quiz.GetDailyQuizChallengeStatus;

public sealed record GetDailyQuizChallengeStatusQuery(Guid UserId) : IRequest<Result<DailyQuizChallengeStatusDto>>;

/// <summary>
/// <see cref="CompletedToday"/> is true once the user has STARTED (not necessarily finished)
/// today's daily challenge session — that's exactly what the unique
/// (UserId, DailyChallengeDateUtc) constraint blocks a second attempt on, in
/// <c>StartQuizSessionCommandHandler</c>, so this flag reflects the same "already used today's
/// shot" gate rather than requiring the session to have reached Completed.
/// </summary>
public sealed record DailyQuizChallengeStatusDto(string Category, QuizDifficulty Difficulty, bool CompletedToday);
