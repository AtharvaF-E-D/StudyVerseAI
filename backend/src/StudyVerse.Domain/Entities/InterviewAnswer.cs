namespace StudyVerse.Domain.Entities;

/// <summary>
/// One graded answer within an <see cref="InterviewSession"/> — created only once the candidate
/// actually submits a real-time answer to one of the session's selected questions (unlike
/// <c>QuizSessionQuestion</c>, there's no placeholder row before that; the session's fixed question
/// list lives on <see cref="InterviewSession.SelectedQuestionIdsJson"/> instead). Graded
/// synchronously by <c>SubmitAnswerCommandHandler</c> via one <c>IAiChatProvider</c> JSON-mode call
/// per answer against <see cref="InterviewQuestion.WhatGoodAnswersCover"/>.
/// </summary>
public class InterviewAnswer
{
    public Guid Id { get; set; }

    public Guid SessionId { get; set; }

    public Guid QuestionId { get; set; }

    public string AnswerText { get; set; } = string.Empty;

    /// <summary>0-10, set immediately on submission by the AI grader.</summary>
    public int? AiScore { get; set; }

    /// <summary>A concise, real, answer-specific feedback sentence from the AI grader — never
    /// generic filler.</summary>
    public string? AiFeedback { get; set; }

    public DateTime AnsweredAtUtc { get; set; }

    public InterviewSession? Session { get; set; }

    public InterviewQuestion? Question { get; set; }
}
