using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using StudyVerse.Application.Common.Interfaces;
using StudyVerse.Application.Common.Models;
using StudyVerse.Infrastructure.Ai;
using StudyVerse.Infrastructure.Auth;
using StudyVerse.Infrastructure.Caching;
using StudyVerse.Infrastructure.Common;
using StudyVerse.Infrastructure.Email;
using StudyVerse.Infrastructure.External;
using StudyVerse.Infrastructure.Files;
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
        services.Configure<AiOptions>(configuration.GetSection(AiOptions.SectionName));
        services.Configure<OpenAiOptions>(configuration.GetSection(OpenAiOptions.SectionName));
        services.Configure<StorageOptions>(configuration.GetSection(StorageOptions.SectionName));
        services.Configure<GNewsOptions>(configuration.GetSection(GNewsOptions.SectionName));
        services.Configure<Judge0Options>(configuration.GetSection(Judge0Options.SectionName));

        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();
        services.AddSingleton<IRandomProvider, RandomProvider>();
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

        // Singleton: ChatClient (OpenAI SDK) is thread-safe and holds no per-request state.
        // Resolved lazily, so a missing OpenAI:ApiKey only throws when a tutor request actually
        // comes in, not at application startup (Program.cs logs a warning at startup instead).
        services.AddSingleton<IAiChatProvider, OpenAiChatProvider>();

        // Local disk today; swap this one registration for a Cloudflare R2/S3-backed
        // IFileStorageService later (see that interface's doc comment) — no other code changes.
        services.AddSingleton<IFileStorageService, LocalFileStorageService>();

        // Same lazy-API-key-resolution reasoning as IAiChatProvider above.
        services.AddSingleton<INoteGenerationProvider, OpenAiNoteGenerationProvider>();
        services.AddSingleton<ITextExtractionService, DocumentTextExtractionService>();

        // Same lazy-API-key-resolution reasoning as IAiChatProvider above.
        services.AddSingleton<IFlashcardGenerationProvider, OpenAiFlashcardGenerationProvider>();

        services.AddHttpClient(nameof(GNewsProvider), client =>
        {
            client.BaseAddress = new Uri("https://gnews.io/api/v4/");
            client.Timeout = TimeSpan.FromSeconds(15);
        });
        services.AddSingleton<IGNewsProvider, GNewsProvider>();

        // Real Judge0 CE via RapidAPI - base address + the fixed x-rapidapi-host header live here
        // (compile-time constants); the per-request x-rapidapi-key header (configuration-driven) is
        // added by Judge0Provider itself. A generous timeout since `wait=true` blocks on Judge0
        // actually compiling+running the submission before responding.
        services.AddHttpClient(nameof(Judge0Provider), client =>
        {
            client.BaseAddress = new Uri("https://judge0-ce.p.rapidapi.com/");
            client.DefaultRequestHeaders.Add("x-rapidapi-host", "judge0-ce.p.rapidapi.com");
            client.Timeout = TimeSpan.FromSeconds(30);
        });
        services.AddSingleton<IJudge0Provider, Judge0Provider>();

        return services;
    }
}
