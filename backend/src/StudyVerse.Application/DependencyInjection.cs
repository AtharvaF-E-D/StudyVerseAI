using System.Reflection;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using StudyVerse.Application.Common.Behaviors;
using StudyVerse.Application.Common.Interfaces;
using StudyVerse.Application.Common.Services;

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

        return services;
    }
}
