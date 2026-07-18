using MediatR;
using StudyVerse.Domain.Common;
using StudyVerse.Domain.CurrentAffairs;

namespace StudyVerse.Application.Features.CurrentAffairs.GetCategories;

public sealed class GetCategoriesQueryHandler : IRequestHandler<GetCategoriesQuery, Result<IReadOnlyList<string>>>
{
    public Task<Result<IReadOnlyList<string>>> Handle(GetCategoriesQuery request, CancellationToken cancellationToken) =>
        Task.FromResult(Result.Success(NewsCategories.All));
}
