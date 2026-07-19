using MediatR;
using StudyVerse.Application.Features.CodingPractice.Common;
using StudyVerse.Domain.Common;

namespace StudyVerse.Application.Features.CodingPractice.SubmitCode;

public sealed record SubmitCodeCommand(Guid UserId, Guid ProblemId, int LanguageId, string SourceCode)
    : IRequest<Result<SubmitCodeResultDto>>;
