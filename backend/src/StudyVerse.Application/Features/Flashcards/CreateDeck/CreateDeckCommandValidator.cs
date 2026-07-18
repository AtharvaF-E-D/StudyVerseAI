using FluentValidation;

namespace StudyVerse.Application.Features.Flashcards.CreateDeck;

public sealed class CreateDeckCommandValidator : AbstractValidator<CreateDeckCommand>
{
    public CreateDeckCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();

        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);

        RuleFor(x => x.Description).MaximumLength(2000);
    }
}
