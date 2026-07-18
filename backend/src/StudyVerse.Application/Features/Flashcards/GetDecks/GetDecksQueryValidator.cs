using FluentValidation;

namespace StudyVerse.Application.Features.Flashcards.GetDecks;

public sealed class GetDecksQueryValidator : AbstractValidator<GetDecksQuery>
{
    public GetDecksQueryValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
    }
}
