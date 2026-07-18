using MediatR;
using StudyVerse.Domain.Common;

namespace StudyVerse.Application.Features.StudyPlanner.ArchiveStudyPlan;

/// <summary>Manual archive (e.g. the user abandons a plan) - the automatic
/// "one Active plan at a time" archiving that happens inside <c>CreateStudyPlanCommandHandler</c>
/// covers the usual case; this is for a user who just wants to stop following the current plan
/// without starting a new one.</summary>
public sealed record ArchiveStudyPlanCommand(Guid UserId, Guid PlanId) : IRequest<Result>;
