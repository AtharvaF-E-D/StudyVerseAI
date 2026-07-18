using MediatR;
using StudyVerse.Domain.Common;

namespace StudyVerse.Application.Features.MockTests.SubmitMockTestAttempt;

/// <summary>
/// The list of answers is allowed to be incomplete: any question whose id isn't present here is
/// treated as unanswered (and scored wrong) rather than rejecting the whole submission — see
/// <c>SubmitMockTestAttemptCommandHandler</c>.
/// </summary>
public sealed record SubmitMockTestAttemptCommand(Guid UserId, Guid AttemptId, IReadOnlyList<MockTestAnswerInput> Answers)
    : IRequest<Result<SubmitMockTestAttemptResultDto>>;

public sealed record MockTestAnswerInput(Guid QuestionId, int SelectedOptionIndex);

public sealed record SubmitMockTestAttemptResultDto(
    int Score,
    int CorrectCount,
    int TotalQuestions,
    double PercentileRank,
    string AiWeaknessAnalysis);
