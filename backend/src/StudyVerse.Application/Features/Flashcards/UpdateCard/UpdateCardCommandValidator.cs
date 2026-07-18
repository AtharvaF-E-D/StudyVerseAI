using FluentValidation;

namespace StudyVerse.Application.Features.Flashcards.UpdateCard;

public sealed class UpdateCardCommandValidator : AbstractValidator<UpdateCardCommand>
{
    public UpdateCardCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.DeckId).NotEmpty();
        RuleFor(x => x.CardId).NotEmpty();
        RuleFor(x => x.FrontText).NotEmpty().MaximumLength(2000);
        RuleFor(x => x.BackText).NotEmpty().MaximumLength(2000);
        RuleFor(x => x.ImageUrl).MaximumLength(2000);
    }
}
