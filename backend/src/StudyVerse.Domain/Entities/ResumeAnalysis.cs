namespace StudyVerse.Domain.Entities;

/// <summary>
/// One AI-graded resume upload. <see cref="StoredFilePath"/> is the opaque storage key returned by
/// <c>IFileStorageService.SaveAsync</c> — the same PDF/DOCX upload pipeline
/// <c>UploadNoteCommandHandler</c> uses (via <c>ITextExtractionService</c>) to get plain text before
/// the single AI analysis call. <see cref="StrengthsJson"/>/<see cref="WeaknessesJson"/>/
/// <see cref="SuggestionsJson"/> are JSON arrays of strings — the same "AI-owned, always read as one
/// unit" JSON-text-column philosophy as <c>NoteContent</c>'s <c>*Json</c> properties, rather than
/// normalized child tables for content nobody queries piecemeal.
/// </summary>
public class ResumeAnalysis
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public string FileName { get; set; } = string.Empty;

    public string StoredFilePath { get; set; } = string.Empty;

    /// <summary>0-100 overall resume quality score from the AI analysis.</summary>
    public int OverallScore { get; set; }

    /// <summary>JSON array of strings, 3-5 real strengths specific to this resume's content.</summary>
    public string StrengthsJson { get; set; } = "[]";

    /// <summary>JSON array of strings, 3-5 real weaknesses specific to this resume's content.</summary>
    public string WeaknessesJson { get; set; } = "[]";

    /// <summary>JSON array of strings, 3-5 real actionable suggestions specific to this resume's content.</summary>
    public string SuggestionsJson { get; set; } = "[]";

    public DateTime AnalyzedAtUtc { get; set; }

    public User? User { get; set; }
}
