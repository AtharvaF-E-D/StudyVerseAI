using MediatR;
using StudyVerse.Application.Features.CodingPractice.Common;
using StudyVerse.Domain.Common;

namespace StudyVerse.Application.Features.CodingPractice.GetProblem;

public sealed record GetProblemQuery(Guid UserId, Guid ProblemId, int LanguageId)
    : IRequest<Result<CodingProblemDetailDto>>;
