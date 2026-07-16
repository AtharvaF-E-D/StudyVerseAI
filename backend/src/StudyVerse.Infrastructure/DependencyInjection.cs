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

        // Empty/missing connection string => local dev without Redis installed falls back to an
        // in-memory cache (see InMemoryCacheService). docker-compose.yml and CI both set
        // Redis__ConnectionString explicitly, so containerized/CI runs always use real Redis;
        // Staging/Production configs must always provide a real connection string.
        var redisConnectionString = configuration["Redis:ConnectionString"];

        if (string.IsNullOrWhiteSpace(redisConnectionString))
        {
            services.AddMemoryCache();
            services.AddSingleton<ICacheService, InMemoryCacheService>();
        }
        else
        {
            services.AddSingleton<IConnectionMultiplexer>(_ =>
                ConnectionMultiplexer.Connect(redisConnectionString));

            services.AddSingleton<ICacheService, RedisCacheService>();
        }

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
