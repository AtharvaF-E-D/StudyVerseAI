namespace StudyVerse.Domain.Entities;

/// <summary>
/// One question shown within a <see cref="QuizSession"/>, in <see cref="OrderIndex"/> order.
/// Serves two purposes: (1) before it's answered, it's part of the "which questions were shown to
/// this user" record joined against for anti-repetition when selecting a future session's
/// questions (see <c>QuizQuestionSelectionService</c>); (2) once
/// <see cref="SelectedOptionIndex"/>/<see cref="IsCorrect"/>/<see cref="AnsweredAtUtc"/> are
/// filled in, it's the source data for <c>GetQuizReviewQuery</c>.
/// </summary>
public class QuizSessionQuestion
{
    public Guid Id { get; set; }

    public Guid SessionId { get; set; }

    public Guid QuestionId { get; set; }

    public int OrderIndex { get; set; }

    public int? SelectedOptionIndex { get; set; }

    public bool? IsCorrect { get; set; }

    public int? TimeTakenMs { get; set; }

    public DateTime? AnsweredAtUtc { get; set; }

    public QuizSession? Session { get; set; }

    public QuizQuestion? Question { get; set; }
}
