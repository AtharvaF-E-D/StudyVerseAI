using FluentValidation;

namespace StudyVerse.Application.Features.Flashcards.ToggleDeckFavorite;

public sealed class ToggleDeckFavoriteCommandValidator : AbstractValidator<ToggleDeckFavoriteCommand>
{
    public ToggleDeckFavoriteCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.DeckId).NotEmpty();
    }
}
