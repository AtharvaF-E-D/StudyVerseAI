using MediatR;
using Microsoft.EntityFrameworkCore;
using StudyVerse.Application.Common.Interfaces;
using StudyVerse.Application.Features.CurrentAffairs.Common;
using StudyVerse.Domain.Common;
using StudyVerse.Domain.Entities;
using StudyVerse.Domain.Enums;

namespace StudyVerse.Application.Features.CurrentAffairs.GetWeeklyDigest;

public sealed class GetWeeklyDigestQueryHandler : IRequestHandler<GetWeeklyDigestQuery, Result<WeeklyDigestDto>>
{
    private readonly IAppDbContext _db;
    private readonly IAiChatProvider _aiChatProvider;
    private readonly IDateTimeProvider _dateTimeProvider;

    public GetWeeklyDigestQueryHandler(IAppDbContext db, IAiChatProvider aiChatProvider, IDateTimeProvider dateTimeProvider)
    {
        _db = db;
        _aiChatProvider = aiChatProvider;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<Result<WeeklyDigestDto>> Handle(GetWeeklyDigestQuery request, CancellationToken cancellationToken)
    {
        var today = DateOnly.FromDateTime(_dateTimeProvider.UtcNow);
        var weekStart = MostRecentMonday(today);

        var existingDigest = await _db.WeeklyDigests.FirstOrDefaultAsync(d => d.WeekStartDateUtc == weekStart, cancellationToken);
        if (existingDigest is not null)
        {
            return Result.Success(ToDto(existingDigest));
        }

        var weekStartUtc = DateTime.SpecifyKind(weekStart.ToDateTime(TimeOnly.MinValue), DateTimeKind.Utc);
        var weekEndUtc = weekStartUtc.AddDays(7);

        var articleSummaries = await _db.NewsArticles
            .Where(a => a.PublishedAtUtc >= weekStartUtc && a.PublishedAtUtc < weekEndUtc)
            .OrderBy(a => a.Category)
            .ThenByDescending(a => a.PublishedAtUtc)
            .Select(a => new WeeklyDigestArticleSummary(a.Category, a.Title, a.Description))
            .ToListAsync(cancellationToken);

        if (articleSummaries.Count == 0)
        {
            return Result.Failure<WeeklyDigestDto>(
                "Not enough cached news data yet to build this week's digest - browse a few categories first.",
                ResultErrorType.NotFound);
        }

        var prompt = WeeklyDigestPromptBuilder.Build(weekStart, articleSummaries);

        var completion = await _aiChatProvider.GetCompletionAsync([new AiChatMessage(MessageRole.User, prompt)], cancellationToken);

        if (string.IsNullOrWhiteSpace(completion.Content))
        {
            return Result.Failure<WeeklyDigestDto>("The AI didn't return a usable digest. Please try again.");
        }

        var digest = new WeeklyDigest
        {
            Id = Guid.NewGuid(),
            WeekStartDateUtc = weekStart,
            SummaryText = completion.Content.Trim(),
            GeneratedAtUtc = _dateTimeProvider.UtcNow,
        };
        _db.WeeklyDigests.Add(digest);
        await _db.SaveChangesAsync(cancellationToken);

        return Result.Success(ToDto(digest));
    }

    private static WeeklyDigestDto ToDto(WeeklyDigest digest) =>
        new(digest.WeekStartDateUtc, digest.SummaryText, digest.GeneratedAtUtc);

    /// <summary>The Monday on/before <paramref name="date"/> - same convention as
    /// <c>GetWeeklyTasksQueryHandler</c>'s private helper of the same name.</summary>
    private static DateOnly MostRecentMonday(DateOnly date)
    {
        var daysSinceMonday = ((int)date.DayOfWeek - (int)DayOfWeek.Monday + 7) % 7;
        return date.AddDays(-daysSinceMonday);
    }
}
