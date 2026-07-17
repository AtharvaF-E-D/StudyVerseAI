using MediatR;
using Microsoft.EntityFrameworkCore;
using StudyVerse.Application.Common.Interfaces;
using StudyVerse.Domain.Common;
using StudyVerse.Domain.Enums;
using StudyVerse.Domain.Quiz;

namespace StudyVerse.Application.Features.Quiz.GetQuizCategories;

public sealed class GetQuizCategoriesQueryHandler
    : IRequestHandler<GetQuizCategoriesQuery, Result<IReadOnlyList<QuizCategorySummaryDto>>>
{
    private readonly IAppDbContext _db;

    public GetQuizCategoriesQueryHandler(IAppDbContext db)
    {
        _db = db;
    }

    public async Task<Result<IReadOnlyList<QuizCategorySummaryDto>>> Handle(
        GetQuizCategoriesQuery request,
        CancellationToken cancellationToken)
    {
        var counts = await _db.QuizQuestions
            .GroupBy(q => new { q.Category, q.Difficulty })
            .Select(g => new { g.Key.Category, g.Key.Difficulty, Count = g.Count() })
            .ToListAsync(cancellationToken);

        // Iterate the fixed category catalog (not just whatever's in the DB) so a category with
        // zero seeded questions of some difficulty still shows up with a 0 count rather than being
        // silently omitted.
        var summaries = QuizCategories.All
            .Select(category =>
            {
                int CountFor(QuizDifficulty difficulty) =>
                    counts.FirstOrDefault(c => c.Category == category && c.Difficulty == difficulty)?.Count ?? 0;

                var easy = CountFor(QuizDifficulty.Easy);
                var medium = CountFor(QuizDifficulty.Medium);
                var hard = CountFor(QuizDifficulty.Hard);

                return new QuizCategorySummaryDto(category, easy, medium, hard, easy + medium + hard);
            })
            .ToList();

        return Result.Success<IReadOnlyList<QuizCategorySummaryDto>>(summaries);
    }
}
