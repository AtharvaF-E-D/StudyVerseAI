using FluentValidation;

namespace StudyVerse.Application.Features.Flashcards.AddCard;

public sealed class AddCardCommandValidator : AbstractValidator<AddCardCommand>
{
    public AddCardCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.DeckId).NotEmpty();
        RuleFor(x => x.FrontText).NotEmpty().MaximumLength(2000);
        RuleFor(x => x.BackText).NotEmpty().MaximumLength(2000);
        RuleFor(x => x.ImageUrl).MaximumLength(2000);
    }
}
