using StudyVerse.Domain.Enums;

namespace StudyVerse.Api.Contracts;

public sealed record StartInterviewSessionRequest(InterviewQuestionType Type);

public sealed record SubmitInterviewAnswerRequest(Guid QuestionId, string AnswerText);
