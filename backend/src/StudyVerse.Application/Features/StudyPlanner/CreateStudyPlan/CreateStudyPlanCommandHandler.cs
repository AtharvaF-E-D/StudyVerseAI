using MediatR;
using Microsoft.EntityFrameworkCore;
using StudyVerse.Application.Common.Interfaces;
using StudyVerse.Application.Features.StudyPlanner.Common;
using StudyVerse.Domain.Common;
using StudyVerse.Domain.Entities;
using StudyVerse.Domain.Enums;

namespace StudyVerse.Application.Features.StudyPlanner.CreateStudyPlan;

/// <summary>
/// Archives any existing Active plan for the user, calls <see cref="IAiChatProvider"/> once (JSON
/// mode) with a prompt built by <c>StudyPlanPromptBuilder</c>, and persists the new plan plus every
/// AI-generated <c>StudyPlanTask</c> row - all before the single <c>SaveChangesAsync</c> at the end,
/// same "compute in memory, commit once" shape as <c>SubmitMockTestAttemptCommandHandler</c>.
/// </summary>
public sealed class CreateStudyPlanCommandHandler : IRequestHandler<CreateStudyPlanCommand, Result<CreateStudyPlanResultDto>>
{
    private readonly IAppDbContext _db;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IAiChatProvider _aiChatProvider;

    public CreateStudyPlanCommandHandler(IAppDbContext db, IDateTimeProvider dateTimeProvider, IAiChatProvider aiChatProvider)
    {
        _db = db;
        _dateTimeProvider = dateTimeProvider;
        _aiChatProvider = aiChatProvider;
    }

    public async Task<Result<CreateStudyPlanResultDto>> Handle(CreateStudyPlanCommand request, CancellationToken cancellationToken)
    {
        var today = DateOnly.FromDateTime(_dateTimeProvider.UtcNow);

        if (request.ExamDate <= today)
        {
            return Result.Failure<CreateStudyPlanResultDto>("Exam date must be in the future.");
        }

        var planEndDate = StudyPlanPromptBuilder.GetPlanEndDate(today, request.ExamDate);

        var prompt = StudyPlanPromptBuilder.Build(
            today, request.ExamDate, planEndDate, request.Subjects, request.WeakTopics, request.HoursPerDayMinutes);

        var completion = await _aiChatProvider.GetCompletionAsync(
            [new AiChatMessage(MessageRole.User, prompt)],
            cancellationToken,
            requireJsonObjectResponse: true);

        var generatedTasks = StudyPlanAiResponseParser.Parse(completion.Content, today, planEndDate);

        if (generatedTasks.Count == 0)
        {
            return Result.Failure<CreateStudyPlanResultDto>(
                "The AI didn't return a usable study plan. Please try again.");
        }

        // Only one Active plan per user at a time - archive whatever was previously active before
        // creating the new one (see StudyPlan's doc comment).
        var existingActivePlans = await _db.StudyPlans
            .Where(p => p.UserId == request.UserId && p.Status == StudyPlanStatus.Active)
            .ToListAsync(cancellationToken);

        foreach (var existingPlan in existingActivePlans)
        {
            existingPlan.Status = StudyPlanStatus.Archived;
        }

        var plan = new StudyPlan
        {
            Id = Guid.NewGuid(),
            UserId = request.UserId,
            ExamDate = request.ExamDate,
            SubjectsJson = StudyPlanJsonHelper.Serialize(request.Subjects),
            WeakTopicsJson = StudyPlanJsonHelper.Serialize(request.WeakTopics),
            HoursPerDayMinutes = request.HoursPerDayMinutes,
            Status = StudyPlanStatus.Active,
            CreatedAtUtc = _dateTimeProvider.UtcNow,
        };
        _db.StudyPlans.Add(plan);

        foreach (var generatedTask in generatedTasks)
        {
            _db.StudyPlanTasks.Add(new StudyPlanTask
            {
                Id = Guid.NewGuid(),
                PlanId = plan.Id,
                ScheduledDateUtc = generatedTask.Date,
                Subject = generatedTask.Subject,
                Topic = generatedTask.Topic,
                DurationMinutes = generatedTask.DurationMinutes,
                IsWeakTopic = generatedTask.IsWeakTopic,
                Status = StudyPlanTaskStatus.Pending,
            });
        }

        await _db.SaveChangesAsync(cancellationToken);

        return Result.Success(new CreateStudyPlanResultDto(plan.Id, plan.ExamDate, generatedTasks.Count));
    }
}
