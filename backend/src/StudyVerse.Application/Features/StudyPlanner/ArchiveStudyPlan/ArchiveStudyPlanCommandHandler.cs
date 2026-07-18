using MediatR;
using Microsoft.EntityFrameworkCore;
using StudyVerse.Application.Common.Interfaces;
using StudyVerse.Domain.Common;
using StudyVerse.Domain.Enums;

namespace StudyVerse.Application.Features.StudyPlanner.ArchiveStudyPlan;

public sealed class ArchiveStudyPlanCommandHandler : IRequestHandler<ArchiveStudyPlanCommand, Result>
{
    private readonly IAppDbContext _db;

    public ArchiveStudyPlanCommandHandler(IAppDbContext db)
    {
        _db = db;
    }

    public async Task<Result> Handle(ArchiveStudyPlanCommand request, CancellationToken cancellationToken)
    {
        var plan = await _db.StudyPlans.FirstOrDefaultAsync(
            p => p.Id == request.PlanId && p.UserId == request.UserId, cancellationToken);

        if (plan is null)
        {
            return Result.Failure("Study plan not found.", ResultErrorType.NotFound);
        }

        if (plan.Status != StudyPlanStatus.Archived)
        {
            plan.Status = StudyPlanStatus.Archived;
            await _db.SaveChangesAsync(cancellationToken);
        }

        return Result.Success();
    }
}
