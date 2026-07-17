namespace StudyVerse.Application.Features.Quiz.Common;

/// <summary>A question projected for gameplay: no <c>CorrectOptionIndex</c> or <c>Explanation</c> — never let the client see the answer before it answers.</summary>
public sealed record QuizQuestionOptionsDto(Guid Id, string QuestionText, IReadOnlyList<string> Options);

public sealed record PowerUpsAvailableDto(bool FiftyFifty, bool ExtraTime);
