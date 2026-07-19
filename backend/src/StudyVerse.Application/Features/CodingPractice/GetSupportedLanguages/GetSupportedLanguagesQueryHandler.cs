using MediatR;
using StudyVerse.Application.Features.CodingPractice.Common;
using StudyVerse.Domain.Common;

namespace StudyVerse.Application.Features.CodingPractice.GetSupportedLanguages;

public sealed class GetSupportedLanguagesQueryHandler
    : IRequestHandler<GetSupportedLanguagesQuery, Result<IReadOnlyList<SupportedLanguageDto>>>
{
    public Task<Result<IReadOnlyList<SupportedLanguageDto>>> Handle(GetSupportedLanguagesQuery request, CancellationToken cancellationToken) =>
        Task.FromResult(Result.Success(SupportedLanguages.All));
}
