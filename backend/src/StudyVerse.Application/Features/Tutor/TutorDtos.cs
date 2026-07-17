using StudyVerse.Domain.Enums;

namespace StudyVerse.Application.Features.Tutor;

public sealed record MessageDto(Guid Id, MessageRole Role, string Content, DateTime CreatedAtUtc);

public sealed record ConversationSummaryDto(
    Guid Id,
    string Title,
    bool IsBookmarked,
    DateTime UpdatedAtUtc,
    string? LastMessagePreview);

public sealed record AiUsageDto(int TokensUsedToday, int DailyLimit, int Remaining);
