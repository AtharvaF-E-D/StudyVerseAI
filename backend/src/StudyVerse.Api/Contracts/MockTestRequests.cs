namespace StudyVerse.Api.Contracts;

public sealed record StartMockTestAttemptRequest(Guid TemplateId);

public sealed record SubmitMockTestAttemptRequest(IReadOnlyList<MockTestAnswerRequest> Answers);

public sealed record MockTestAnswerRequest(Guid QuestionId, int SelectedOptionIndex);
