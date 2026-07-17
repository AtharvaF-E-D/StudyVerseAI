using FluentValidation;

namespace StudyVerse.Application.Features.Tutor.CreateConversation;

public sealed class CreateConversationCommandValidator : AbstractValidator<CreateConversationCommand>
{
    public CreateConversationCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
    }
}
