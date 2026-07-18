using FluentValidation;

namespace StudyVerse.Application.Features.Flashcards.GenerateDeckFromTopic;

public sealed class GenerateDeckFromTopicCommandValidator : AbstractValidator<GenerateDeckFromTopicCommand>
{
    /// <summary>Caps how many cards a single AI call is asked to generate, so a runaway request
    /// can't blow the model's output budget or the deck size out of proportion.</summary>
    public const int MaxCardCount = 20;

    public GenerateDeckFromTopicCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();

        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);

        RuleFor(x => x.Topic).NotEmpty().MaximumLength(500);

        RuleFor(x => x.CardCount).InclusiveBetween(1, MaxCardCount);
    }
}
