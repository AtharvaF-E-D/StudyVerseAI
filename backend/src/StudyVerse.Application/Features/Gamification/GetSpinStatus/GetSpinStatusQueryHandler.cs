using MediatR;
using Microsoft.EntityFrameworkCore;
using StudyVerse.Application.Common.Interfaces;
using StudyVerse.Domain.Common;

namespace StudyVerse.Application.Features.Gamification.GetSpinStatus;

public sealed class GetSpinStatusQueryHandler : IRequestHandler<GetSpinStatusQuery, Result<SpinStatusDto>>
{
    private readonly IAppDbContext _db;
    private readonly IDateTimeProvider _dateTimeProvider;

    public GetSpinStatusQueryHandler(IAppDbContext db, IDateTimeProvider dateTimeProvider)
    {
        _db = db;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<Result<SpinStatusDto>> Handle(GetSpinStatusQuery request, CancellationToken cancellationToken)
    {
        var userExists = await _db.Users.AnyAsync(u => u.Id == request.UserId, cancellationToken);
        if (!userExists)
        {
            return Result.Failure<SpinStatusDto>("User not found.", ResultErrorType.NotFound);
        }

        var today = DateOnly.FromDateTime(_dateTimeProvider.UtcNow);

        var todaysSpin = await _db.SpinResults
            .Where(s => s.UserId == request.UserId && s.SpinDateUtc == today)
            .Select(s => s.PrizeLabel)
            .FirstOrDefaultAsync(cancellationToken);

        var result = new SpinStatusDto(todaysSpin is not null, todaysSpin);

        return Result.Success(result);
    }
}
