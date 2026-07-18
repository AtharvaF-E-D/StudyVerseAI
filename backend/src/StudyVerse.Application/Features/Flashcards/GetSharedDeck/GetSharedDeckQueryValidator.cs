using FluentValidation;

namespace StudyVerse.Application.Features.Flashcards.GetSharedDeck;

public sealed class GetSharedDeckQueryValidator : AbstractValidator<GetSharedDeckQuery>
{
    public GetSharedDeckQueryValidator()
    {
        RuleFor(x => x.ShareToken).NotEmpty();
    }
}
