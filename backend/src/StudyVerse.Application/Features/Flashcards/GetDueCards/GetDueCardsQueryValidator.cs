using FluentValidation;

namespace StudyVerse.Application.Features.Flashcards.GetDueCards;

public sealed class GetDueCardsQueryValidator : AbstractValidator<GetDueCardsQuery>
{
    public GetDueCardsQueryValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
    }
}
