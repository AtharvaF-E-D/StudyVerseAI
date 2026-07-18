using MediatR;
using Microsoft.EntityFrameworkCore;
using StudyVerse.Application.Common.Interfaces;
using StudyVerse.Domain.Common;
using StudyVerse.Domain.Enums;

namespace StudyVerse.Application.Features.StudyPlanner.CompleteTask;

public sealed class CompleteTaskCommandHandler : IRequestHandler<CompleteTaskCommand, Result>
{
    private readonly IAppDbContext _db;
    private readonly IDateTimeProvider _dateTimeProvider;

    public CompleteTaskCommandHandler(IAppDbContext db, IDateTimeProvider dateTimeProvider)
    {
        _db = db;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<Result> Handle(CompleteTaskCommand request, CancellationToken cancellationToken)
    {
        // Ownership is checked via the parent plan's UserId, same pattern as
        // ReviewCardCommandHandler checking a card's ownership via c.Deck!.UserId.
        var task = await _db.StudyPlanTasks.FirstOrDefaultAsync(
            t => t.Id == request.TaskId && t.Plan!.UserId == request.UserId, cancellationToken);

        if (task is null)
        {
            return Result.Failure("Study plan task not found.", ResultErrorType.NotFound);
        }

        if (task.Status != StudyPlanTaskStatus.Completed)
        {
            task.Status = StudyPlanTaskStatus.Completed;
            task.CompletedAtUtc = _dateTimeProvider.UtcNow;
            await _db.SaveChangesAsync(cancellationToken);
        }

        return Result.Success();
    }
}
