using MediatR;
using StudyVerse.Domain.Common;

namespace StudyVerse.Application.Features.Quiz.UseFiftyFifty;

public sealed record UseFiftyFiftyCommand(Guid UserId, Guid SessionId) : IRequest<Result<FiftyFiftyResultDto>>;

/// <summary>The two incorrect option indexes (0-3) the client should hide for the current question.</summary>
public sealed record FiftyFiftyResultDto(IReadOnlyList<int> HiddenOptionIndexes);
