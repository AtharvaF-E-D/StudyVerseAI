using FluentValidation;

namespace StudyVerse.Application.Features.Flashcards.UnshareDeck;

public sealed class UnshareDeckCommandValidator : AbstractValidator<UnshareDeckCommand>
{
    public UnshareDeckCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.DeckId).NotEmpty();
    }
}
