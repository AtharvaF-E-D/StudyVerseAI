using MediatR;
using Microsoft.EntityFrameworkCore;
using StudyVerse.Application.Common.Interfaces;
using StudyVerse.Application.Features.InterviewPrep.Common;
using StudyVerse.Domain.Common;
using StudyVerse.Domain.Enums;

namespace StudyVerse.Application.Features.InterviewPrep.GetInterviewCategories;

public sealed class GetInterviewCategoriesQueryHandler
    : IRequestHandler<GetInterviewCategoriesQuery, Result<IReadOnlyList<InterviewCategoryDto>>>
{
    private readonly IAppDbContext _db;

    public GetInterviewCategoriesQueryHandler(IAppDbContext db)
    {
        _db = db;
    }

    public async Task<Result<IReadOnlyList<InterviewCategoryDto>>> Handle(
        GetInterviewCategoriesQuery request,
        CancellationToken cancellationToken)
    {
        var counts = await _db.InterviewQuestions
            .GroupBy(q => q.Type)
            .Select(g => new { Type = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        // Iterate the fixed enum (not just whatever's in the DB) so a type with zero seeded
        // questions still shows up with a 0 count rather than being silently omitted.
        var categories = Enum.GetValues<InterviewQuestionType>()
            .Select(type => new InterviewCategoryDto(
                type,
                DisplayName(type),
                counts.FirstOrDefault(c => c.Type == type)?.Count ?? 0))
            .ToList();

        return Result.Success<IReadOnlyList<InterviewCategoryDto>>(categories);
    }

    private static string DisplayName(InterviewQuestionType type) => type switch
    {
        InterviewQuestionType.Hr => "HR",
        InterviewQuestionType.Technical => "Technical",
        InterviewQuestionType.Behavioral => "Behavioral",
        _ => type.ToString(),
    };
}
