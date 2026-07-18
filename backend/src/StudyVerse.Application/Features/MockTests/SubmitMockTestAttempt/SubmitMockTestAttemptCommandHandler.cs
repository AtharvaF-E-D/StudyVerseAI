using MediatR;
using Microsoft.EntityFrameworkCore;
using StudyVerse.Application.Common.Interfaces;
using StudyVerse.Application.Features.MockTests.Common;
using StudyVerse.Domain.Common;
using StudyVerse.Domain.Entities;
using StudyVerse.Domain.Enums;

namespace StudyVerse.Application.Features.MockTests.SubmitMockTestAttempt;

/// <summary>
/// Grades every question in one shot (unlike the Rapid Fire Quiz's answer-as-you-go
/// <c>SubmitAnswerCommandHandler</c>): looks up the real correct answers server-side, scores
/// unanswered questions as wrong, computes the percentage <c>Score</c> and
/// <c>MockTestPercentileCalculator</c>-based <c>PercentileRank</c> against every other Submitted
/// attempt for the same template, then calls <see cref="IAiChatProvider"/> once with a prompt built
/// from the wrong answers (grouped by category) for a short weakness analysis. Everything —
/// including the AI call — happens before the single <c>SaveChangesAsync</c> at the end, so if the
/// AI call throws, nothing about this submission is persisted and the caller can safely retry
/// (same "compute in memory, commit once" shape as <c>SendMessageCommandHandler</c>).
/// </summary>
public sealed class SubmitMockTestAttemptCommandHandler
    : IRequestHandler<SubmitMockTestAttemptCommand, Result<SubmitMockTestAttemptResultDto>>
{
    private readonly IAppDbContext _db;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IAiChatProvider _aiChatProvider;

    public SubmitMockTestAttemptCommandHandler(
        IAppDbContext db,
        IDateTimeProvider dateTimeProvider,
        IAiChatProvider aiChatProvider)
    {
        _db = db;
        _dateTimeProvider = dateTimeProvider;
        _aiChatProvider = aiChatProvider;
    }

    public async Task<Result<SubmitMockTestAttemptResultDto>> Handle(
        SubmitMockTestAttemptCommand request,
        CancellationToken cancellationToken)
    {
        var attempt = await _db.MockTestAttempts.FirstOrDefaultAsync(
            a => a.Id == request.AttemptId && a.UserId == request.UserId,
            cancellationToken);

        if (attempt is null)
        {
            return Result.Failure<SubmitMockTestAttemptResultDto>(
                "Mock test attempt not found.", ResultErrorType.NotFound);
        }

        if (attempt.Status != MockTestAttemptStatus.InProgress)
        {
            return Result.Failure<SubmitMockTestAttemptResultDto>(
                "This mock test attempt has already been submitted.", ResultErrorType.Conflict);
        }

        var answerRows = await _db.MockTestAttemptAnswers
            .Where(a => a.AttemptId == attempt.Id)
            .OrderBy(a => a.OrderIndex)
            .ToListAsync(cancellationToken);

        var questionIds = answerRows.Select(a => a.QuestionId).ToList();
        var questionsById = await _db.QuizQuestions
            .Where(q => questionIds.Contains(q.Id))
            .ToDictionaryAsync(q => q.Id, cancellationToken);

        // GroupBy+First (not straight ToDictionary) so a client accidentally sending the same
        // QuestionId twice can never blow up the whole submission - the first value given wins.
        var selectedByQuestionId = request.Answers
            .GroupBy(a => a.QuestionId)
            .ToDictionary(g => g.Key, g => g.First().SelectedOptionIndex);

        var correctCount = 0;
        var wrongAnswers = new List<MockTestWrongAnswer>();

        foreach (var row in answerRows)
        {
            var question = questionsById[row.QuestionId];
            var hasSelection = selectedByQuestionId.TryGetValue(row.QuestionId, out var selectedOptionIndex);

            row.SelectedOptionIndex = hasSelection ? selectedOptionIndex : null;
            row.IsCorrect = hasSelection && selectedOptionIndex == question.CorrectOptionIndex;

            if (row.IsCorrect)
            {
                correctCount++;
            }
            else
            {
                var options = new[] { question.OptionA, question.OptionB, question.OptionC, question.OptionD };
                var selectedText = hasSelection && selectedOptionIndex is >= 0 and <= 3
                    ? options[selectedOptionIndex]
                    : "(no answer given)";

                wrongAnswers.Add(new MockTestWrongAnswer(
                    question.Category,
                    question.QuestionText,
                    selectedText,
                    options[question.CorrectOptionIndex]));
            }
        }

        var totalQuestions = answerRows.Count;
        var score = totalQuestions == 0 ? 0 : (int)Math.Round(100.0 * correctCount / totalQuestions);

        var otherSubmittedScores = await _db.MockTestAttempts
            .Where(a => a.TemplateId == attempt.TemplateId
                        && a.Status == MockTestAttemptStatus.Submitted
                        && a.Id != attempt.Id)
            .Select(a => a.Score!.Value)
            .ToListAsync(cancellationToken);

        var percentileRank = MockTestPercentileCalculator.Calculate(score, otherSubmittedScores);

        // A clean sweep has nothing to analyze - skip the AI round trip entirely rather than asking
        // the model to invent "weaknesses" out of thin air.
        var aiWeaknessAnalysis = wrongAnswers.Count == 0
            ? "Perfect score - no weak areas found on this attempt. Keep up the great work and try a harder or mixed-subject mock test next to keep challenging yourself."
            : await GenerateWeaknessAnalysisAsync(wrongAnswers, cancellationToken);

        var now = _dateTimeProvider.UtcNow;
        attempt.Status = MockTestAttemptStatus.Submitted;
        attempt.SubmittedAtUtc = now;
        attempt.Score = score;
        attempt.CorrectCount = correctCount;
        attempt.PercentileRank = percentileRank;
        attempt.AiWeaknessAnalysis = aiWeaknessAnalysis;

        await _db.SaveChangesAsync(cancellationToken);

        var result = new SubmitMockTestAttemptResultDto(score, correctCount, totalQuestions, percentileRank, aiWeaknessAnalysis);

        return Result.Success(result);
    }

    private async Task<string> GenerateWeaknessAnalysisAsync(
        IReadOnlyList<MockTestWrongAnswer> wrongAnswers,
        CancellationToken cancellationToken)
    {
        var prompt = MockTestWeaknessPromptBuilder.Build(wrongAnswers);
        var completion = await _aiChatProvider.GetCompletionAsync(
            [new AiChatMessage(MessageRole.User, prompt)],
            cancellationToken);

        return completion.Content;
    }
}
