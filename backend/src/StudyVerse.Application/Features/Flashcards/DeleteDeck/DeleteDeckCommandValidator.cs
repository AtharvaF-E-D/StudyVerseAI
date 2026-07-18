using FluentValidation;

namespace StudyVerse.Application.Features.Flashcards.DeleteDeck;

public sealed class DeleteDeckCommandValidator : AbstractValidator<DeleteDeckCommand>
{
    public DeleteDeckCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.DeckId).NotEmpty();
    }
}
