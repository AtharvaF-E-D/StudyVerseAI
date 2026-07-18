using FluentValidation;

namespace StudyVerse.Application.Features.Flashcards.ReviewCard;

public sealed class ReviewCardCommandValidator : AbstractValidator<ReviewCardCommand>
{
    public ReviewCardCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.CardId).NotEmpty();
        RuleFor(x => x.Quality).IsInEnum();
    }
}
