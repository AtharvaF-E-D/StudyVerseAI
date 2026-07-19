using MediatR;
using StudyVerse.Application.Features.CodingPractice.Common;
using StudyVerse.Domain.Common;

namespace StudyVerse.Application.Features.CodingPractice.GetSubmissions;

/// <summary>This user's submission history, newest first, optionally scoped to one problem via
/// <see cref="ProblemId"/>. Ownership isn't really applicable to problems themselves (they're
/// global), but submission history is always scoped to <see cref="UserId"/> - never another user's
/// submissions, even for the same problem.</summary>
public sealed record GetSubmissionsQuery(Guid UserId, Guid? ProblemId) : IRequest<Result<IReadOnlyList<CodeSubmissionDto>>>;
