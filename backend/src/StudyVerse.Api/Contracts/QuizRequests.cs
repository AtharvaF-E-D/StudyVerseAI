using StudyVerse.Domain.Enums;

namespace StudyVerse.Api.Contracts;

public sealed record StartQuizSessionRequest(string Category, QuizDifficulty Difficulty, bool IsDailyChallenge);

public sealed record SubmitAnswerRequest(Guid QuestionId, int SelectedOptionIndex, int? TimeTakenMs);
