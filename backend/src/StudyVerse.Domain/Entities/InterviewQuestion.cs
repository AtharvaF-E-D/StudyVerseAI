using StudyVerse.Domain.Enums;

namespace StudyVerse.Domain.Entities;

/// <summary>
/// One real, hand-written question in the Interview Prep question bank. Seeded via EF Core
/// <c>HasData</c> (see <c>InterviewQuestionSeedData</c> in Infrastructure) with stable hardcoded
/// ids, the same reasoning as <see cref="QuizQuestion"/>/<see cref="CodingProblem"/> — not
/// AI-generated at seed time.
/// </summary>
public class InterviewQuestion
{
    public Guid Id { get; set; }

    public InterviewQuestionType Type { get; set; }

    public string QuestionText { get; set; } = string.Empty;

    /// <summary>
    /// A short internal note on what a strong answer should touch on — feeds the AI grader's prompt
    /// (see <c>InterviewAnswerGradingPromptBuilder</c>) so it grades against a concrete rubric
    /// instead of grading blind. Never sent to the client — <c>GetInterviewSessionQueryHandler</c>/
    /// <c>StartInterviewSessionCommandHandler</c> project questions without this field, the same
    /// anti-leak reasoning <see cref="QuizQuestion.CorrectOptionIndex"/> gets.
    /// </summary>
    public string WhatGoodAnswersCover { get; set; } = string.Empty;

    public DateTime CreatedAtUtc { get; set; }
}
