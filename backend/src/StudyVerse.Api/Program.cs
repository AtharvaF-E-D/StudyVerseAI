using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using Asp.Versioning;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using StudyVerse.Api.Middleware;
using StudyVerse.Application;
using StudyVerse.Application.Common.Models;
using StudyVerse.Infrastructure;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting StudyVerse API");

    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((context, services, loggerConfiguration) => loggerConfiguration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext());

    // ---- MVC + API versioning ----
    builder.Services
        .AddControllers()
        .AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
        });

    builder.Services
        .AddApiVersioning(options =>
        {
            options.DefaultApiVersion = new ApiVersion(1, 0);
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.ReportApiVersions = true;
            options.ApiVersionReader = new UrlSegmentApiVersionReader();
        })
        .AddMvc()
        .AddApiExplorer(options =>
        {
            options.GroupNameFormat = "'v'VVV";
            options.SubstituteApiVersionInUrl = true;
        });

    // ---- Application + Infrastructure ----
    builder.Services.AddApplication();
    builder.Services.AddInfrastructure(builder.Configuration);

    // ---- JWT bearer authentication (bound from the same JwtOptions the token service signs with) ----
    var jwtOptions = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()
        ?? throw new InvalidOperationException("Missing required configuration section 'Jwt'.");

    if (string.IsNullOrWhiteSpace(jwtOptions.SigningKey))
    {
        throw new InvalidOperationException("Jwt:SigningKey must be configured (env var Jwt__SigningKey in production).");
    }

    builder.Services
        .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            // Keep the original JWT claim names ("sub", not the legacy .NET ClaimTypes URIs).
            options.MapInboundClaims = false;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = jwtOptions.Issuer,
                ValidateAudience = true,
                ValidAudience = jwtOptions.Audience,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SigningKey)),
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromSeconds(30),
            };
        });

    builder.Services.AddAuthorization();

    // ---- OpenAI (AI Tutor) ----
    // Not a hard startup failure (many flows don't touch the tutor at all) — OpenAiChatProvider
    // is a lazily-resolved singleton and throws its own clear error the first time a tutor
    // request actually needs it if this is still missing.
    var openAiApiKey = builder.Configuration["OpenAI:ApiKey"];
    if (string.IsNullOrWhiteSpace(openAiApiKey))
    {
        Log.Warning(
            "OpenAI:ApiKey is not configured — AI tutor endpoints will fail until it's set " +
            "(`dotnet user-secrets set OpenAI:ApiKey <key>` in Development, or the OpenAI__ApiKey " +
            "environment variable in Staging/Production).");
    }

    // ---- GNews (Current Affairs) ----
    // Not a hard startup failure, same reasoning as OpenAI:ApiKey above — GNewsProvider itself
    // logs and returns an empty list per-request rather than throwing if this is still missing.
    var gNewsApiKey = builder.Configuration["GNews:ApiKey"];
    if (string.IsNullOrWhiteSpace(gNewsApiKey))
    {
        Log.Warning(
            "GNews:ApiKey is not configured — Current Affairs endpoints will return no fresh " +
            "articles until it's set (`dotnet user-secrets set GNews:ApiKey <key>` in Development, " +
            "or the GNews__ApiKey environment variable in Staging/Production).");
    }

    // ---- Judge0 (Coding Practice grading) ----
    // Not a hard startup failure, same reasoning as GNews:ApiKey above — Judge0Provider itself logs
    // and returns an "Error" status per-submission rather than throwing if this is still missing.
    var judge0ApiKey = builder.Configuration["Judge0:RapidApiKey"];
    if (string.IsNullOrWhiteSpace(judge0ApiKey))
    {
        Log.Warning(
            "Judge0:RapidApiKey is not configured — Coding Practice submissions will fail to grade " +
            "until it's set (`dotnet user-secrets set Judge0:RapidApiKey <key>` in Development, or " +
            "the Judge0__RapidApiKey environment variable in Staging/Production).");
    }

    // ---- Swagger / OpenAPI ----
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new OpenApiInfo { Title = "StudyVerse API", Version = "v1" });

        options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\"",
            Name = "Authorization",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
        });

        options.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" },
                },
                Array.Empty<string>()
            },
        });
    });

    // ---- Health checks ----
    var healthChecksBuilder = builder.Services.AddHealthChecks()
        .AddNpgSql(
            builder.Configuration.GetConnectionString("Postgres")!,
            name: "postgres",
            tags: ["ready"]);

    var redisConnectionStringForHealthCheck = builder.Configuration["Redis:ConnectionString"];
    if (!string.IsNullOrWhiteSpace(redisConnectionStringForHealthCheck))
    {
        healthChecksBuilder.AddRedis(redisConnectionStringForHealthCheck, name: "redis", tags: ["ready"]);
    }
    else
    {
        // Matches the in-memory ICacheService fallback registered by AddInfrastructure when no
        // Redis connection string is configured (local dev without Redis installed).
        healthChecksBuilder.AddCheck(
            "redis",
            () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy(
                "Using in-memory cache fallback (no Redis configured)."),
            tags: ["ready"]);
    }

    // ---- Rate limiting: fixed window per client IP on sensitive auth endpoints ----
    builder.Services.AddRateLimiter(options =>
    {
        options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
        options.AddFixedWindowLimiter("auth", limiterOptions =>
        {
            limiterOptions.PermitLimit = 10;
            limiterOptions.Window = TimeSpan.FromMinutes(1);
            limiterOptions.QueueLimit = 0;
            limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        });
    });

    // ---- CORS ----
    // Permissive for Phase 1 local/dev mobile testing only — lock this down to explicit,
    // configuration-driven origins before shipping to staging/production.
    const string corsPolicyName = "MobileDev";
    builder.Services.AddCors(options =>
    {
        options.AddPolicy(corsPolicyName, policy =>
        {
            policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
        });
    });

    // ---- Global exception handling -> application/problem+json ----
    builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
    builder.Services.AddProblemDetails();

    var app = builder.Build();

    app.UseExceptionHandler();

    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "StudyVerse API v1");
    });

    app.UseHttpsRedirection();
    app.UseCors(corsPolicyName);
    app.UseRateLimiter();
    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();

    app.MapHealthChecks("/health/live", new HealthCheckOptions
    {
        Predicate = _ => false,
    });

    app.MapHealthChecks("/health/ready", new HealthCheckOptions
    {
        Predicate = check => check.Tags.Contains("ready"),
    });

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "StudyVerse API terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

/// <summary>Partial Program class so <c>WebApplicationFactory&lt;Program&gt;</c> can be used in integration tests.</summary>
public partial class Program;
