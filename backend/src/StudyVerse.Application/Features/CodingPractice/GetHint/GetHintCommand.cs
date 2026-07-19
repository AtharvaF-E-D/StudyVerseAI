using MediatR;
using StudyVerse.Domain.Common;

namespace StudyVerse.Application.Features.CodingPractice.GetHint;

public sealed record GetHintCommand(Guid UserId, Guid ProblemId, string CurrentCode) : IRequest<Result<CodingHintDto>>;

public sealed record CodingHintDto(string Hint);
