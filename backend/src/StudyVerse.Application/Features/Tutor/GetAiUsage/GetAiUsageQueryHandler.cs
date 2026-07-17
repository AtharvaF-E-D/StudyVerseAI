using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using StudyVerse.Application.Common.Interfaces;
using StudyVerse.Application.Common.Models;
using StudyVerse.Application.Features.Tutor.Common;
using StudyVerse.Domain.Common;

namespace StudyVerse.Application.Features.Tutor.GetAiUsage;

public sealed class GetAiUsageQueryHandler : IRequestHandler<GetAiUsageQuery, Result<AiUsageDto>>
{
    private readonly IAppDbContext _db;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly AiOptions _aiOptions;

    public GetAiUsageQueryHandler(IAppDbContext db, IDateTimeProvider dateTimeProvider, IOptions<AiOptions> aiOptions)
    {
        _db = db;
        _dateTimeProvider = dateTimeProvider;
        _aiOptions = aiOptions.Value;
    }

    public async Task<Result<AiUsageDto>> Handle(GetAiUsageQuery request, CancellationToken cancellationToken)
    {
        var userExists = await _db.Users.AnyAsync(u => u.Id == request.UserId, cancellationToken);
        if (!userExists)
        {
            return Result.Failure<AiUsageDto>("User not found.", ResultErrorType.NotFound);
        }

        var progress = await _db.UserProgresses.FirstOrDefaultAsync(p => p.UserId == request.UserId, cancellationToken);
        var today = DateOnly.FromDateTime(_dateTimeProvider.UtcNow);

        var tokensUsedToday = AiUsagePolicy.GetTokensUsedToday(progress, today);
        var dailyLimit = _aiOptions.DailyTokenLimit;
        var remaining = Math.Max(0, dailyLimit - tokensUsedToday);

        return Result.Success(new AiUsageDto(tokensUsedToday, dailyLimit, remaining));
    }
}
