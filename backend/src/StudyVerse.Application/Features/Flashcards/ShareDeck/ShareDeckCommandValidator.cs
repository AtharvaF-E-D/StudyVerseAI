using FluentValidation;

namespace StudyVerse.Application.Features.Flashcards.ShareDeck;

public sealed class ShareDeckCommandValidator : AbstractValidator<ShareDeckCommand>
{
    public ShareDeckCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.DeckId).NotEmpty();
    }
}
