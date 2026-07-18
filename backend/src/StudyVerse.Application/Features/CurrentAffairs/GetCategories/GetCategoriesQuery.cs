using MediatR;
using StudyVerse.Domain.Common;

namespace StudyVerse.Application.Features.CurrentAffairs.GetCategories;

/// <summary>The fixed 9-category catalog (<c>CurrentAffairs.NewsCategories.All</c>) - no DB or
/// GNews call needed, purely static data.</summary>
public sealed record GetCategoriesQuery : IRequest<Result<IReadOnlyList<string>>>;
