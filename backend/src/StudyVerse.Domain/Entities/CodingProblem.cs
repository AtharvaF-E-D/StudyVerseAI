using StudyVerse.Domain.Enums;

namespace StudyVerse.Domain.Entities;

/// <summary>
/// One real, hand-written coding problem in the Coding Practice bank. Seeded via EF Core
/// <c>HasData</c> (see <c>CodingProblemSeedData</c> in Infrastructure) with stable hardcoded ids,
/// the same reasoning as <see cref="QuizQuestion"/>/<c>ChallengeTemplate</c>. Every problem defines
/// a plain stdin/stdout contract (e.g. "read a line of space-separated integers, print their sum")
/// so a single Judge0 submission per test case can grade it purely by comparing stdout to
/// <see cref="CodingProblemTestCase.ExpectedOutput"/> — no custom checker code.
/// </summary>
public class CodingProblem
{
    public Guid Id { get; set; }

    public string Title { get; set; } = string.Empty;

    /// <summary>The full problem statement (plain text/markdown) including its stdin/stdout contract.</summary>
    public string Description { get; set; } = string.Empty;

    public CodingDifficulty Difficulty { get; set; }

    /// <summary>Free-text grouping, e.g. "Arrays", "Strings", "Math", "Recursion", "Data Structures".</summary>
    public string Category { get; set; } = string.Empty;

    /// <summary>Flags a subset of the bank as classic technical-interview staples (e.g. Two Sum,
    /// Valid Parentheses) — surfaced via <c>GetProblemsQuery</c>'s <c>InterviewOnly</c> filter.</summary>
    public bool IsInterviewQuestion { get; set; }

    /// <summary>
    /// A JSON object mapping Judge0 language id (as a string key, e.g. <c>"109"</c>) to a minimal
    /// starter/signature snippet for that language — just enough for the user to see the expected
    /// stdin/stdout shape, not a real solution scaffold. Not every supported language necessarily
    /// has an entry; <c>GetProblemQueryHandler</c> falls back to a documented default language when
    /// the requested one is missing (see that handler's doc comment).
    /// </summary>
    public string StarterCodeJson { get; set; } = "{}";

    public DateTime CreatedAtUtc { get; set; }

    public ICollection<CodingProblemTestCase> TestCases { get; set; } = new List<CodingProblemTestCase>();
}
