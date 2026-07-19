using System.Reflection;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using StudyVerse.Application.Common.Behaviors;
using StudyVerse.Application.Common.Interfaces;
using StudyVerse.Application.Common.Services;
using StudyVerse.Application.Features.Gamification.Common;
using StudyVerse.Application.Features.StudyPlanner.Common;

namespace StudyVerse.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var applicationAssembly = Assembly.GetExecutingAssembly();

        services.AddMediatR(config =>
        {
            config.RegisterServicesFromAssembly(applicationAssembly);
            config.AddOpenBehavior(typeof(LoggingBehavior<,>));
            config.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });

        services.AddValidatorsFromAssembly(applicationAssembly);

        services.AddAutoMapper(cfg => cfg.AddMaps(applicationAssembly));

        services.AddScoped<IStreakService, StreakService>();

        // Plain concrete class, not behind an interface (see its own doc comment for why) - the
        // Study Planner's missed-task recovery pass, injected directly by GetTodayTasksQueryHandler
        // and GetActivePlanQueryHandler.
        services.AddScoped<MissedTaskRecoveryService>();

        // Same "plain concrete class" reasoning as MissedTaskRecoveryService above - Phase 13
        // Gamification's lazy badge/mission evaluators, shared by their respective Get*QueryHandlers
        // and the gamification summary.
        services.AddScoped<BadgeEvaluationService>();
        services.AddScoped<MissionProgressService>();

        return services;
    }
}
