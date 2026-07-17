using FluentValidation;

namespace StudyVerse.Application.Features.Tutor.GetConversations;

public sealed class GetConversationsQueryValidator : AbstractValidator<GetConversationsQuery>
{
    public GetConversationsQueryValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.Take).GreaterThan(0);
        RuleFor(x => x.Search).MaximumLength(200);
    }
}
