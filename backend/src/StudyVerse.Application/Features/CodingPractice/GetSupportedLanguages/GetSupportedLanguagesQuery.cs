using MediatR;
using StudyVerse.Application.Features.CodingPractice.Common;
using StudyVerse.Domain.Common;

namespace StudyVerse.Application.Features.CodingPractice.GetSupportedLanguages;

/// <summary>The small, fixed Judge0 language-id map (<see cref="SupportedLanguages.All"/>) - no DB
/// or Judge0 call needed, purely static data.</summary>
public sealed record GetSupportedLanguagesQuery : IRequest<Result<IReadOnlyList<SupportedLanguageDto>>>;
