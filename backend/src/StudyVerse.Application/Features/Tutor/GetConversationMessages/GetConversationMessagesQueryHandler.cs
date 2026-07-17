using MediatR;
using Microsoft.EntityFrameworkCore;
using StudyVerse.Application.Common.Interfaces;
using StudyVerse.Domain.Common;

namespace StudyVerse.Application.Features.Tutor.GetConversationMessages;

public sealed class GetConversationMessagesQueryHandler
    : IRequestHandler<GetConversationMessagesQuery, Result<IReadOnlyList<MessageDto>>>
{
    private readonly IAppDbContext _db;

    public GetConversationMessagesQueryHandler(IAppDbContext db)
    {
        _db = db;
    }

    public async Task<Result<IReadOnlyList<MessageDto>>> Handle(
        GetConversationMessagesQuery request,
        CancellationToken cancellationToken)
    {
        var conversationOwned = await _db.Conversations.AnyAsync(
            c => c.Id == request.ConversationId && c.UserId == request.UserId,
            cancellationToken);

        if (!conversationOwned)
        {
            return Result.Failure<IReadOnlyList<MessageDto>>("Conversation not found.", ResultErrorType.NotFound);
        }

        var messages = await _db.Messages
            .Where(m => m.ConversationId == request.ConversationId)
            .OrderBy(m => m.CreatedAtUtc)
            .Select(m => new MessageDto(m.Id, m.Role, m.Content, m.CreatedAtUtc))
            .ToListAsync(cancellationToken);

        return Result.Success<IReadOnlyList<MessageDto>>(messages);
    }
}
