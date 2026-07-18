using FluentValidation;

namespace StudyVerse.Application.Features.Flashcards.GetDeck;

public sealed class GetDeckQueryValidator : AbstractValidator<GetDeckQuery>
{
    public GetDeckQueryValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.DeckId).NotEmpty();
    }
}
