using FluentValidation;

namespace StudyVerse.Application.Features.StudyPlanner.CreateStudyPlan;

public sealed class CreateStudyPlanCommandValidator : AbstractValidator<CreateStudyPlanCommand>
{
    /// <summary>Sanity cap on how many subjects/weak topics a single AI plan-generation call is
    /// asked to juggle, so a runaway request can't blow the prompt out of proportion.</summary>
    public const int MaxSubjectCount = 20;

    public CreateStudyPlanCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();

        // ExamDate must be in the future - checked in the handler (it needs IDateTimeProvider's
        // clock, which validators in this codebase never take a dependency on), not here.

        // Null-safe .Must() (rather than a separate RuleFor(x => x.Subjects.Count)) so a null
        // Subjects list fails validation cleanly instead of throwing a NullReferenceException while
        // FluentValidation evaluates the rule.
        RuleFor(x => x.Subjects)
            .Must(s => s is { Count: >= 1 and <= MaxSubjectCount })
            .WithMessage($"Provide between 1 and {MaxSubjectCount} subjects.");
        RuleForEach(x => x.Subjects).NotEmpty().MaximumLength(200);

        // Weak topics are optional (an empty list is fine - not every student has flagged any yet),
        // so this only rejects a missing/oversized list, not an empty one.
        RuleFor(x => x.WeakTopics)
            .Must(s => s is { Count: <= MaxSubjectCount })
            .WithMessage($"Weak topics must be provided (an empty list is fine) and number at most {MaxSubjectCount}.");
        RuleForEach(x => x.WeakTopics).NotEmpty().MaximumLength(200);

        // 15 minutes to 16 hours/day - generous but not unbounded, sanity bounds for the prompt's
        // daily budget rather than a hard product requirement.
        RuleFor(x => x.HoursPerDayMinutes).InclusiveBetween(15, 960);
    }
}
