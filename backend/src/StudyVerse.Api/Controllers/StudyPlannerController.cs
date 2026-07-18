using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudyVerse.Api.Contracts;
using StudyVerse.Application.Features.StudyPlanner.ArchiveStudyPlan;
using StudyVerse.Application.Features.StudyPlanner.CompleteTask;
using StudyVerse.Application.Features.StudyPlanner.CreateStudyPlan;
using StudyVerse.Application.Features.StudyPlanner.GetActivePlan;
using StudyVerse.Application.Features.StudyPlanner.GetTodayTasks;
using StudyVerse.Application.Features.StudyPlanner.GetWeeklyTasks;

namespace StudyVerse.Api.Controllers;

/// <summary>Study Planner: AI-generated day-by-day study schedules counting down to an exam date,
/// with automatic missed-task recovery (see <c>MissedTaskRecoveryService</c>).</summary>
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/studyplanner")]
[Authorize]
public sealed class StudyPlannerController : ApiControllerBase
{
    [HttpPost("plans")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CreatePlan([FromBody] CreateStudyPlanRequest request, CancellationToken cancellationToken)
    {
        if (CurrentUserId is not { } userId)
        {
            return Unauthorized();
        }

        var result = await Mediator.Send(
            new CreateStudyPlanCommand(userId, request.ExamDate, request.Subjects, request.WeakTopics, request.HoursPerDayMinutes),
            cancellationToken);

        return FromResult(result, dto => Ok(dto));
    }

    [HttpGet("plans/active")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetActivePlan(CancellationToken cancellationToken)
    {
        if (CurrentUserId is not { } userId)
        {
            return Unauthorized();
        }

        var result = await Mediator.Send(new GetActivePlanQuery(userId), cancellationToken);

        return FromResult(result, dto => Ok(dto));
    }

    [HttpGet("tasks/today")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTodayTasks(CancellationToken cancellationToken)
    {
        if (CurrentUserId is not { } userId)
        {
            return Unauthorized();
        }

        var result = await Mediator.Send(new GetTodayTasksQuery(userId), cancellationToken);

        return FromResult(result, dto => Ok(dto));
    }

    [HttpGet("tasks/week")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetWeeklyTasks([FromQuery] DateOnly? weekStartDate, CancellationToken cancellationToken)
    {
        if (CurrentUserId is not { } userId)
        {
            return Unauthorized();
        }

        var result = await Mediator.Send(new GetWeeklyTasksQuery(userId, weekStartDate), cancellationToken);

        return FromResult(result, dto => Ok(dto));
    }

    [HttpPost("tasks/{id:guid}/complete")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CompleteTask(Guid id, CancellationToken cancellationToken)
    {
        if (CurrentUserId is not { } userId)
        {
            return Unauthorized();
        }

        var result = await Mediator.Send(new CompleteTaskCommand(userId, id), cancellationToken);

        return FromResult(result, () => Ok(new { message = "Task marked complete." }));
    }

    [HttpPost("plans/{id:guid}/archive")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ArchivePlan(Guid id, CancellationToken cancellationToken)
    {
        if (CurrentUserId is not { } userId)
        {
            return Unauthorized();
        }

        var result = await Mediator.Send(new ArchiveStudyPlanCommand(userId, id), cancellationToken);

        return FromResult(result, () => Ok(new { message = "Study plan archived." }));
    }
}
