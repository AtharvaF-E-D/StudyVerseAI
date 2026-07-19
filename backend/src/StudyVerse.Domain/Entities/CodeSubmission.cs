using StudyVerse.Domain.Enums;

namespace StudyVerse.Domain.Entities;

/// <summary>
/// One graded attempt at a <see cref="CodingProblem"/> — persisted for every submission (not just
/// accepted ones), so <c>GetSubmissionsQuery</c> can show a real history and
/// <c>SubmitCodeCommandHandler</c> can check "has this user ever been Accepted on this problem
/// before" to decide whether to award XP/coins (first-ever accepted solve only — see that
/// handler's doc comment).
/// </summary>
public class CodeSubmission
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public Guid ProblemId { get; set; }

    /// <summary>Judge0's language id (e.g. 109 = Python 3.13.2) — see <c>SupportedLanguages</c>.</summary>
    public int LanguageId { get; set; }

    public string SourceCode { get; set; } = string.Empty;

    public CodeSubmissionStatus Status { get; set; }

    public int TestsPassed { get; set; }

    public int TotalTests { get; set; }

    public DateTime SubmittedAtUtc { get; set; }

    public User? User { get; set; }

    public CodingProblem? Problem { get; set; }
}
