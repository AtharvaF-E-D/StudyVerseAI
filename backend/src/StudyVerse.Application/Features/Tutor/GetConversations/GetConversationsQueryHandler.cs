using MediatR;
using Microsoft.EntityFrameworkCore;
using StudyVerse.Application.Common.Interfaces;
using StudyVerse.Domain.Common;

namespace StudyVerse.Application.Features.Tutor.GetConversations;

public sealed class GetConversationsQueryHandler
    : IRequestHandler<GetConversationsQuery, Result<IReadOnlyList<ConversationSummaryDto>>>
{
    private const int PreviewLength = 80;

    private readonly IAppDbContext _db;

    public GetConversationsQueryHandler(IAppDbContext db)
    {
        _db = db;
    }

    public async Task<Result<IReadOnlyList<ConversationSummaryDto>>> Handle(
        GetConversationsQuery request,
        CancellationToken cancellationToken)
    {
        var query = _db.Conversations.Where(c => c.UserId == request.UserId);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            // `.ToLower().Contains(...)` (rather than `EF.Functions.ILike`, which is
            // Npgsql-only) translates on both the real Postgres provider and the InMemory
            // provider the unit tests run against.
            var term = request.Search.Trim().ToLowerInvariant();
            query = query.Where(c => c.Title.ToLower().Contains(term));
        }

        var conversations = await query
            .OrderByDescending(c => c.UpdatedAtUtc)
            .Take(request.Take)
            .ToListAsync(cancellationToken);

        var conversationIds = conversations.Select(c => c.Id).ToList();

        // Materialize the matching messages ordered newest-first, then take the first per
        // conversation client-side — avoids relying on GroupBy-then-First() translating to SQL,
        // which isn't reliably supported across providers.
        var recentMessages = await _db.Messages
            .Where(m => conversationIds.Contains(m.ConversationId))
            .OrderByDescending(m => m.CreatedAtUtc)
            .Select(m => new { m.ConversationId, m.Content })
            .ToListAsync(cancellationToken);

        var previewByConversation = recentMessages
            .GroupBy(m => m.ConversationId)
            .ToDictionary(g => g.Key, g => g.First().Content);

        var summaries = conversations
            .Select(c => new ConversationSummaryDto(
                c.Id,
                c.Title,
                c.IsBookmarked,
                c.UpdatedAtUtc,
                previewByConversation.TryGetValue(c.Id, out var lastContent) ? Truncate(lastContent, PreviewLength) : null))
            .ToList();

        return Result.Success<IReadOnlyList<ConversationSummaryDto>>(summaries);
    }

    private static string Truncate(string content, int maxLength) =>
        content.Length <= maxLength ? content : content[..maxLength] + "…";
}
