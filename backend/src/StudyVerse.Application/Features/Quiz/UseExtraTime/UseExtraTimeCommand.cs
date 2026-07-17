using MediatR;
using StudyVerse.Domain.Common;

namespace StudyVerse.Application.Features.Quiz.UseExtraTime;

public sealed record UseExtraTimeCommand(Guid UserId, Guid SessionId) : IRequest<Result<UseExtraTimeResultDto>>;

/// <summary>
/// Confirms the extra-time power-up was activated. The actual extra seconds are a client-side
/// timer concern — this command's only job is to prevent the same session using it twice.
/// </summary>
public sealed record UseExtraTimeResultDto(bool ExtraTimeActivated);
