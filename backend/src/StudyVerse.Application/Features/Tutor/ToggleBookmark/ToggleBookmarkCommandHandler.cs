using MediatR;
using Microsoft.EntityFrameworkCore;
using StudyVerse.Application.Common.Interfaces;
using StudyVerse.Domain.Common;

namespace StudyVerse.Application.Features.Tutor.ToggleBookmark;

public sealed class ToggleBookmarkCommandHandler : IRequestHandler<ToggleBookmarkCommand, Result<bool>>
{
    private readonly IAppDbContext _db;

    public ToggleBookmarkCommandHandler(IAppDbContext db)
    {
        _db = db;
    }

    public async Task<Result<bool>> Handle(ToggleBookmarkCommand request, CancellationToken cancellationToken)
    {
        var conversation = await _db.Conversations.FirstOrDefaultAsync(
            c => c.Id == request.ConversationId && c.UserId == request.UserId,
            cancellationToken);

        if (conversation is null)
        {
            return Result.Failure<bool>("Conversation not found.", ResultErrorType.NotFound);
        }

        // Deliberately does NOT touch UpdatedAtUtc: that timestamp reflects chat activity for
        // the conversation list's sort order, not organizational actions like bookmarking.
        conversation.IsBookmarked = !conversation.IsBookmarked;

        await _db.SaveChangesAsync(cancellationToken);

        return Result.Success(conversation.IsBookmarked);
    }
}
