using FluentValidation;

namespace StudyVerse.Application.Features.Flashcards.GetFlashcardStats;

public sealed class GetFlashcardStatsQueryValidator : AbstractValidator<GetFlashcardStatsQuery>
{
    public GetFlashcardStatsQueryValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
    }
}
