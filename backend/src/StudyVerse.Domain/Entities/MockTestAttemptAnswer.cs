namespace StudyVerse.Domain.Entities;

/// <summary>
/// One question shown within a <see cref="MockTestAttempt"/>, in <see cref="OrderIndex"/> order.
/// A placeholder row is created for every selected question at attempt-start time (unlike
/// <see cref="QuizSessionQuestion"/>, which is also created up front but answered one at a time as
/// the session progresses) — <see cref="SelectedOptionIndex"/>/<see cref="IsCorrect"/> stay at
/// their defaults (null / false) until <c>SubmitMockTestAttemptCommandHandler</c> grades every row
/// at once on submit. Unanswered questions on submit simply keep <see cref="SelectedOptionIndex"/>
/// null and are scored wrong.
/// </summary>
public class MockTestAttemptAnswer
{
    public Guid Id { get; set; }

    public Guid AttemptId { get; set; }

    public Guid QuestionId { get; set; }

    public int OrderIndex { get; set; }

    public int? SelectedOptionIndex { get; set; }

    /// <summary>Computed at submit time; meaningless (always false) while the attempt is still InProgress.</summary>
    public bool IsCorrect { get; set; }

    public MockTestAttempt? Attempt { get; set; }

    public QuizQuestion? Question { get; set; }
}
