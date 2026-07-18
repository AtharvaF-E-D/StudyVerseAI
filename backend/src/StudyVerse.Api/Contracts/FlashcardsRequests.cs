using StudyVerse.Domain.Enums;

namespace StudyVerse.Api.Contracts;

public sealed record CreateDeckRequest(string Title, string? Description);

public sealed record GenerateDeckFromTopicRequest(string Title, string Topic, int CardCount);

public sealed record AddCardRequest(string FrontText, string BackText, string? ImageUrl);

public sealed record UpdateCardRequest(string FrontText, string BackText, string? ImageUrl);

public sealed record ReviewCardRequest(ReviewQuality Quality);
