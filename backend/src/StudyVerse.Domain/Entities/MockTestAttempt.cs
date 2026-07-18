using StudyVerse.Domain.Enums;

namespace StudyVerse.Domain.Entities;

/// <summary>
/// One playthrough of a Mock Test: a fixed-size snapshot of questions drawn from the shared
/// <see cref="QuizQuestion"/> bank per a <see cref="StudyVerse.Domain.MockTests.MockTestTemplate"/>,
/// answered all at once and graded server-side on submit (unlike the Rapid Fire Quiz's
/// answer-as-you-go loop). <see cref="Score"/>/<see cref="CorrectCount"/>/<see cref="PercentileRank"/>/
/// <see cref="AiWeaknessAnalysis"/> are all null until <see cref="Status"/> becomes
/// <see cref="MockTestAttemptStatus.Submitted"/>, at which point they're set exactly once —
/// <c>SubmitMockTestAttemptCommandHandler</c> rejects a second submit attempt, so they never change
/// again after that.
/// </summary>
public class MockTestAttempt
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    /// <summary>Matches the <c>Id</c> of one of the static <see cref="StudyVerse.Domain.MockTests.MockTestTemplate"/>
    /// entries - not a foreign key to a database-backed table, since there is no template table.</summary>
    public Guid TemplateId { get; set; }

    public MockTestAttemptStatus Status { get; set; } = MockTestAttemptStatus.InProgress;

    public DateTime StartedAtUtc { get; set; }

    public DateTime? SubmittedAtUtc { get; set; }

    /// <summary>Percentage score (0-100), rounded to the nearest whole point: 100 * CorrectCount / TotalQuestions.</summary>
    public int? Score { get; set; }

    public int CorrectCount { get; set; }

    public int TotalQuestions { get; set; }

    /// <summary>
    /// Where this attempt's <see cref="Score"/> ranks among every other Submitted attempt at the
    /// same <see cref="TemplateId"/>, as a 0-100 percentage (see
    /// <c>MockTestPercentileCalculator</c> for the exact "mean of exclusive/inclusive rank" formula).
    /// If this was the first-ever submitted attempt for the template, it's set to 100 (top of an
    /// n=1 field) rather than left null or set to some undefined value.
    /// </summary>
    public double? PercentileRank { get; set; }

    /// <summary>
    /// A short AI-generated (via the Application layer's <c>IAiChatProvider</c>) analysis of which
    /// topic areas this attempt's wrong answers suggest need work, plus one concrete study
    /// suggestion. Null until submitted.
    /// </summary>
    public string? AiWeaknessAnalysis { get; set; }

    public User? User { get; set; }

    public List<MockTestAttemptAnswer> Answers { get; set; } = [];
}
