using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using StudyVerse.Application.Common.Interfaces;
using StudyVerse.Application.Common.Models;
using StudyVerse.Infrastructure.Auth;
using StudyVerse.Infrastructure.Caching;
using StudyVerse.Infrastructure.Common;
using StudyVerse.Infrastructure.Email;
using StudyVerse.Infrastructure.External;
using StudyVerse.Infrastructure.Options;
using StudyVerse.Infrastructure.Otp;
using StudyVerse.Infrastructure.Persistence;

namespace StudyVerse.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("Postgres")));

        services.AddScoped<IAppDbContext>(sp => sp.GetRequiredService<AppDbContext>());

        var redisConnectionString = configuration["Redis:ConnectionString"]
            ?? throw new InvalidOperationException("Missing required configuration value 'Redis:ConnectionString'.");

        services.AddSingleton<IConnectionMultiplexer>(_ =>
            ConnectionMultiplexer.Connect(redisConnectionString));

        services.AddSingleton<ICacheService, RedisCacheService>();

        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        services.Configure<AppUrlOptions>(configuration.GetSection(AppUrlOptions.SectionName));
        services.Configure<GoogleOptions>(configuration.GetSection(GoogleOptions.SectionName));
        services.Configure<AppleOptions>(configuration.GetSection(AppleOptions.SectionName));

        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();
        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        services.AddSingleton<IJwtTokenService, JwtTokenService>();

        services.AddSingleton<IEmailSender, LoggingEmailSender>();
        services.AddSingleton<IOtpSender, LoggingOtpSender>();

        services.AddSingleton<IGoogleTokenValidator, GoogleTokenValidator>();

        services.AddHttpClient(nameof(AppleTokenValidator), client =>
        {
            client.Timeout = TimeSpan.FromSeconds(10);
        });
        services.AddSingleton<IAppleTokenValidator, AppleTokenValidator>();

        return services;
    }
}
