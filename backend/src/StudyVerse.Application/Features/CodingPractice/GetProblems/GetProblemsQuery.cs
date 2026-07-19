using MediatR;
using StudyVerse.Application.Features.CodingPractice.Common;
using StudyVerse.Domain.Common;

namespace StudyVerse.Application.Features.CodingPractice.GetProblems;

/// <summary>
/// Lists the coding problem bank, optionally filtered by difficulty/category/interview-only.
/// <see cref="Difficulty"/> is a free-text query param (parsed against
/// <see cref="StudyVerse.Domain.Enums.CodingDifficulty"/> case-insensitively by the handler, not an
/// enum here) since it arrives as an ASP.NET <c>[FromQuery]</c> string.
/// </summary>
public sealed record GetProblemsQuery(Guid UserId, string? Difficulty, string? Category, bool? InterviewOnly)
    : IRequest<Result<IReadOnlyList<CodingProblemSummaryDto>>>;
