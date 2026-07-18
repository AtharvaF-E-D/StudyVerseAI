using StudyVerse.Domain.Enums;

namespace StudyVerse.Application.Features.MockTests.Common;

public sealed record MockTestTemplateDto(
    Guid Id,
    string Title,
    string Description,
    string Category,
    int QuestionCount,
    int DurationMinutes);

/// <summary>A question projected for gameplay: no <c>CorrectOptionIndex</c> or <c>Explanation</c> — never let the client see the answer before it submits.</summary>
public sealed record MockTestQuestionOptionsDto(Guid Id, string QuestionText, IReadOnlyList<string> Options);

public sealed record MockTestAttemptListItemDto(
    Guid AttemptId,
    Guid TemplateId,
    string TemplateTitle,
    MockTestAttemptStatus Status,
    int? Score,
    int CorrectCount,
    int TotalQuestions,
    double? PercentileRank,
    DateTime StartedAtUtc,
    DateTime? SubmittedAtUtc);

public sealed record MockTestAttemptDetailDto(
    Guid AttemptId,
    Guid TemplateId,
    string TemplateTitle,
    string Category,
    MockTestAttemptStatus Status,
    DateTime StartedAtUtc,
    DateTime? SubmittedAtUtc,
    int? Score,
    int CorrectCount,
    int TotalQuestions,
    double? PercentileRank,
    string? AiWeaknessAnalysis);

public sealed record MockTestReviewQuestionDto(
    Guid QuestionId,
    int OrderIndex,
    string QuestionText,
    IReadOnlyList<string> Options,
    int CorrectOptionIndex,
    int? SelectedOptionIndex,
    bool IsCorrect,
    string Explanation);

public sealed record MockTestReviewDto(
    Guid AttemptId,
    Guid TemplateId,
    string TemplateTitle,
    int Score,
    int CorrectCount,
    int TotalQuestions,
    double? PercentileRank,
    string? AiWeaknessAnalysis,
    IReadOnlyList<MockTestReviewQuestionDto> Questions);
