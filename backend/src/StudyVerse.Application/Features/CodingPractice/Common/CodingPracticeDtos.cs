using StudyVerse.Domain.Enums;

namespace StudyVerse.Application.Features.CodingPractice.Common;

public sealed record CodingProblemSummaryDto(
    Guid Id,
    string Title,
    CodingDifficulty Difficulty,
    string Category,
    bool IsInterviewQuestion,
    bool IsSolved);

public sealed record SampleTestCaseDto(string Input, string ExpectedOutput);

/// <summary>
/// Full problem detail. <see cref="SampleTestCases"/> only ever contains test cases with
/// <c>IsSample == true</c> - non-sample (hidden) cases are never sent to the client, here or
/// anywhere else (see <c>CodingProblemTestCase</c>'s doc comment).
/// </summary>
public sealed record CodingProblemDetailDto(
    Guid Id,
    string Title,
    string Description,
    CodingDifficulty Difficulty,
    string Category,
    bool IsInterviewQuestion,
    IReadOnlyList<SampleTestCaseDto> SampleTestCases,
    int StarterLanguageId,
    string StarterCode,
    bool IsSolved);

/// <summary>
/// One test case's grading result. <see cref="Input"/>/<see cref="ExpectedOutput"/>/<see cref="ActualOutput"/>
/// are only populated when <see cref="IsSample"/> is true - a hidden test case's result only ever
/// reveals <see cref="Passed"/>, never its content (same anti-cheating reasoning quiz answers use).
/// </summary>
public sealed record TestCaseResultDto(
    string? Input,
    string? ExpectedOutput,
    string? ActualOutput,
    bool Passed,
    bool IsSample);

public sealed record SubmitCodeResultDto(
    CodeSubmissionStatus Status,
    int TestsPassed,
    int TotalTests,
    IReadOnlyList<TestCaseResultDto> Results,
    int XpAwarded,
    int CoinsAwarded,
    bool AlreadySolved);

public sealed record DailyCodingChallengeDto(Guid ProblemId, string Title, CodingDifficulty Difficulty);

public sealed record CodeSubmissionDto(
    Guid Id,
    Guid ProblemId,
    string ProblemTitle,
    int LanguageId,
    CodeSubmissionStatus Status,
    int TestsPassed,
    int TotalTests,
    DateTime SubmittedAtUtc);

public sealed record SolvedByDifficultyDto(int Easy, int Medium, int Hard);

public sealed record CodingStatsDto(
    int TotalSolved,
    SolvedByDifficultyDto SolvedByDifficulty,
    int TotalSubmissions,
    int CurrentDailyStreak);
