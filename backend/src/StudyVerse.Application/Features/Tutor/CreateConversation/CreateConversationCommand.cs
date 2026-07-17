using MediatR;
using StudyVerse.Domain.Common;

namespace StudyVerse.Application.Features.Tutor.CreateConversation;

public sealed record CreateConversationCommand(Guid UserId) : IRequest<Result<CreateConversationResultDto>>;

public sealed record CreateConversationResultDto(Guid Id, string Title, DateTime CreatedAtUtc);
