namespace StudyVerse.Application.Features.Notifications;

public sealed record NotificationDto(
    Guid Id,
    string Title,
    string Body,
    DateTime CreatedAtUtc,
    DateTime? ReadAtUtc);
