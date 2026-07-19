using MediatR;
using StudyVerse.Application.Features.InterviewPrep.Common;
using StudyVerse.Domain.Common;

namespace StudyVerse.Application.Features.InterviewPrep.GetResumeAnalyses;

public sealed record GetResumeAnalysesQuery(Guid UserId) : IRequest<Result<IReadOnlyList<ResumeAnalysisDto>>>;
