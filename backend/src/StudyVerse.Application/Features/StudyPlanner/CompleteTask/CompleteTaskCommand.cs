using MediatR;
using StudyVerse.Domain.Common;

namespace StudyVerse.Application.Features.StudyPlanner.CompleteTask;

/// <summary>Marks a task Completed; ownership is checked via its parent plan's UserId. Idempotent:
/// completing an already-Completed task is a no-op success, not an error, and never overwrites the
/// original CompletedAtUtc.</summary>
public sealed record CompleteTaskCommand(Guid UserId, Guid TaskId) : IRequest<Result>;
