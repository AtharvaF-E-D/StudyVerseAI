using FluentValidation;
using StudyVerse.Domain.Quiz;

namespace StudyVerse.Application.Features.Quiz.StartQuizSession;

public sealed class StartQuizSessionCommandValidator : AbstractValidator<StartQuizSessionCommand>
{
    public StartQuizSessionCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();

        RuleFor(x => x.Category)
            .NotEmpty()
            .Must(category => QuizCategories.All.Contains(category))
            .WithMessage($"Category must be one of: {string.Join(", ", QuizCategories.All)}.");

        RuleFor(x => x.Difficulty).IsInEnum();
    }
}
