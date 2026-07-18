using MediatR;
using StudyVerse.Application.Features.StudyPlanner.Common;
using StudyVerse.Domain.Common;

namespace StudyVerse.Application.Features.StudyPlanner.GetActivePlan;

/// <summary>Runs the missed-task recovery pass (see <c>MissedTaskRecoveryService</c>) before
/// summarizing the user's current Active plan, so progress/missed counts are always up to date the
/// moment this is called - never a Result.Failure/null shrug, always current, live numbers.</summary>
public sealed record GetActivePlanQuery(Guid UserId) : IRequest<Result<ActiveStudyPlanDto>>;
