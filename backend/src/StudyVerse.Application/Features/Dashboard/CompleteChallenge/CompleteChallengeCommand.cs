using MediatR;
using StudyVerse.Domain.Common;

namespace StudyVerse.Application.Features.Dashboard.CompleteChallenge;

public sealed record CompleteChallengeCommand(Guid UserId, Guid ChallengeTemplateId)
    : IRequest<Result<CompleteChallengeResultDto>>;

public sealed record CompleteChallengeResultDto(int XpAwarded, int CoinsAwarded, int NewXpTotal, int NewCoinsTotal, int NewLevel);
