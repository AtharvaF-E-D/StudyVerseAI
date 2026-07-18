using FluentValidation;

namespace StudyVerse.Application.Features.Flashcards.DeleteCard;

public sealed class DeleteCardCommandValidator : AbstractValidator<DeleteCardCommand>
{
    public DeleteCardCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.DeckId).NotEmpty();
        RuleFor(x => x.CardId).NotEmpty();
    }
}
