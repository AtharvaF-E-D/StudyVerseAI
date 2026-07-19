namespace StudyVerse.Domain.Enums;

/// <summary>The three interview-prep categories a <see cref="StudyVerse.Domain.Entities.InterviewQuestion"/>
/// belongs to and a <see cref="StudyVerse.Domain.Entities.InterviewSession"/> is scoped to. Technical
/// questions are deliberately language-agnostic (e.g. data structures/complexity concepts, not
/// syntax) so the bank works regardless of which language a candidate codes in — see
/// <c>InterviewQuestionSeedData</c>.</summary>
public enum InterviewQuestionType
{
    Hr = 0,
    Technical = 1,
    Behavioral = 2,
}
