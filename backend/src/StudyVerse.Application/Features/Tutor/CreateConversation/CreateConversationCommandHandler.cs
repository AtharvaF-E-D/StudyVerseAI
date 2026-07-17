using MediatR;
using StudyVerse.Application.Common.Interfaces;
using StudyVerse.Domain.Common;
using StudyVerse.Domain.Entities;

namespace StudyVerse.Application.Features.Tutor.CreateConversation;

public sealed class CreateConversationCommandHandler
    : IRequestHandler<CreateConversationCommand, Result<CreateConversationResultDto>>
{
    public const string PlaceholderTitle = "New conversation";

    private readonly IAppDbContext _db;
    private readonly IDateTimeProvider _dateTimeProvider;

    public CreateConversationCommandHandler(IAppDbContext db, IDateTimeProvider dateTimeProvider)
    {
        _db = db;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<Result<CreateConversationResultDto>> Handle(
        CreateConversationCommand request,
        CancellationToken cancellationToken)
    {
        var now = _dateTimeProvider.UtcNow;

        var conversation = new Conversation
        {
            Id = Guid.NewGuid(),
            UserId = request.UserId,
            Title = PlaceholderTitle,
            IsBookmarked = false,
            CreatedAtUtc = now,
            UpdatedAtUtc = now,
        };

        _db.Conversations.Add(conversation);
        await _db.SaveChangesAsync(cancellationToken);

        return Result.Success(new CreateConversationResultDto(conversation.Id, conversation.Title, conversation.CreatedAtUtc));
    }
}
