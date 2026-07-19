using StudyVerse.Domain.Enums;

namespace StudyVerse.Application.Features.InterviewPrep.Common;

public sealed record InterviewCategoryDto(InterviewQuestionType Type, string DisplayName, int QuestionCount);

/// <summary>One question within a session, plus whatever answer/grade has been given so far (all
/// null until the candidate submits an answer for it). Never includes
/// <see cref="StudyVerse.Domain.Entities.InterviewQuestion.WhatGoodAnswersCover"/> — that's an
/// AI-grading-only field, never shown to the candidate.</summary>
public sealed record InterviewSessionQuestionDto(
    Guid QuestionId,
    string QuestionText,
    string? AnswerText,
    int? AiScore,
    string? AiFeedback);

public sealed record InterviewSessionDto(
    Guid Id,
    InterviewQuestionType Type,
    InterviewSessionStatus Status,
    int? OverallScore,
    string? ImprovementPlan,
    DateTime StartedAtUtc,
    DateTime? CompletedAtUtc,
    IReadOnlyList<InterviewSessionQuestionDto> Questions);

/// <summary>Shape returned by <c>GetInterviewSessionsQuery</c>'s history list — no per-question detail.</summary>
public sealed record InterviewSessionSummaryDto(
    Guid Id,
    InterviewQuestionType Type,
    InterviewSessionStatus Status,
    int? OverallScore,
    DateTime StartedAtUtc,
    DateTime? CompletedAtUtc);

public sealed record SubmitAnswerResultDto(int Score, string Feedback);

public sealed record ResumeAnalysisDto(
    Guid Id,
    string FileName,
    int OverallScore,
    IReadOnlyList<string> Strengths,
    IReadOnlyList<string> Weaknesses,
    IReadOnlyList<string> Suggestions,
    DateTime AnalyzedAtUtc);

/// <summary>Null for a type with no completed sessions yet, rather than 0 — a candidate who's never
/// done a Technical session shouldn't look like they scored zero on it.</summary>
public sealed record AverageScoreByTypeDto(double? Hr, double? Technical, double? Behavioral);

public sealed record InterviewStatsDto(int SessionsCompleted, AverageScoreByTypeDto AverageScoreByType, int ResumeAnalysesCount);
