namespace StudyVerse.Application.Features.CodingPractice.Common;

/// <summary>
/// The small, fixed set of Judge0 language ids Coding Practice supports - real, verified-working
/// ids as confirmed against the live Judge0 CE RapidAPI instance during this phase's build (see the
/// phase report). Deliberately a short hardcoded list (not a live call to Judge0's own
/// <c>/languages</c> endpoint) - same reasoning <c>QuizCategories</c> hardcodes its fixed category
/// list rather than deriving it from the DB.
/// </summary>
public static class SupportedLanguages
{
    /// <summary>Python - used as the documented fallback whenever a problem has no starter code for
    /// the language a caller requested (see <c>GetProblemQueryHandler</c>).</summary>
    public const int DefaultLanguageId = 109;

    public static IReadOnlyList<SupportedLanguageDto> All { get; } =
    [
        new(109, "Python (3.13.2)"),
        new(102, "JavaScript (Node.js 22.08.0)"),
        new(91, "Java (JDK 17.0.6)"),
        new(105, "C++ (GCC 14.1.0)"),
        new(51, "C# (Mono 6.6.0.161)"),
    ];

    public static bool IsSupported(int languageId) => All.Any(l => l.LanguageId == languageId);
}

public sealed record SupportedLanguageDto(int LanguageId, string Name);
