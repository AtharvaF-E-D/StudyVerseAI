using MediatR;
using Microsoft.EntityFrameworkCore;
using StudyVerse.Application.Common.Interfaces;
using StudyVerse.Domain.Common;

namespace StudyVerse.Application.Features.Tutor.DeleteConversation;

public sealed class DeleteConversationCommandHandler : IRequestHandler<DeleteConversationCommand, Result>
{
    private readonly IAppDbContext _db;

    public DeleteConversationCommandHandler(IAppDbContext db)
    {
        _db = db;
    }

    public async Task<Result> Handle(DeleteConversationCommand request, CancellationToken cancellationToken)
    {
        var conversation = await _db.Conversations.FirstOrDefaultAsync(
            c => c.Id == request.ConversationId && c.UserId == request.UserId,
            cancellationToken);

        if (conversation is null)
        {
            return Result.Failure("Conversation not found.", ResultErrorType.NotFound);
        }

        // No need to load/remove Messages explicitly: ConversationConfiguration configures a
        // DB-level cascade delete on the Conversation -> Messages relationship.
        _db.Conversations.Remove(conversation);

        await _db.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
