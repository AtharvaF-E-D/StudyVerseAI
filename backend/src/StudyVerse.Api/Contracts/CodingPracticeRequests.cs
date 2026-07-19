namespace StudyVerse.Api.Contracts;

public sealed record SubmitCodeRequest(int LanguageId, string SourceCode);

public sealed record GetHintRequest(string CurrentCode);
