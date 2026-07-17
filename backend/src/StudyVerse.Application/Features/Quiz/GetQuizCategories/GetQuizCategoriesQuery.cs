using MediatR;
using StudyVerse.Domain.Common;

namespace StudyVerse.Application.Features.Quiz.GetQuizCategories;

/// <summary>Distinct categories with per-difficulty question counts, so the client can show what's available.</summary>
public sealed record GetQuizCategoriesQuery : IRequest<Result<IReadOnlyList<QuizCategorySummaryDto>>>;

public sealed record QuizCategorySummaryDto(string Category, int EasyCount, int MediumCount, int HardCount, int TotalCount);
