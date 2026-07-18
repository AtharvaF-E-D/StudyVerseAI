using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StudyVerse.Application.Common.Interfaces;
using StudyVerse.Infrastructure.Options;

namespace StudyVerse.Infrastructure.External;

/// <summary>
/// Calls the real GNews REST API (https://gnews.io/api/v4/) via a plain <see cref="HttpClient"/> -
/// same shape as <see cref="AppleTokenValidator"/> (a plain REST endpoint with no official SDK),
/// registered the same way via <c>AddHttpClient(nameof(GNewsProvider), ...)</c> in
/// <c>DependencyInjection</c>, with the base address set there.
///
/// Free tier notes baked into this implementation: GNews has a 12-hour data delay and a modest
/// daily request quota, which is exactly why callers (<c>GetArticlesByCategoryQuery</c>) cache
/// aggressively rather than calling this on every request - this class itself has no caching of its
/// own, that's the Application layer's job. Any failure (network error, non-2xx response,
/// unparseable body, or a missing API key) is logged and swallowed, returning an empty list - see
/// <see cref="IGNewsProvider"/>'s doc comment for why.
/// </summary>
public sealed class GNewsProvider : IGNewsProvider
{
    private const int MaxArticles = 10;

    private readonly HttpClient _httpClient;
    private readonly GNewsOptions _options;
    private readonly ILogger<GNewsProvider> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public GNewsProvider(IHttpClientFactory httpClientFactory, IOptions<GNewsOptions> options, ILogger<GNewsProvider> logger)
    {
        _httpClient = httpClientFactory.CreateClient(nameof(GNewsProvider));
        _options = options.Value;
        _logger = logger;
    }

    public Task<IReadOnlyList<GNewsArticleDto>> GetTopHeadlinesAsync(string category, CancellationToken cancellationToken = default) =>
        FetchAsync(
            $"top-headlines?category={Uri.EscapeDataString(category)}&lang=en&country=us&max={MaxArticles}&apikey={{0}}",
            cancellationToken);

    public Task<IReadOnlyList<GNewsArticleDto>> SearchAsync(string query, CancellationToken cancellationToken = default) =>
        FetchAsync(
            $"search?q={Uri.EscapeDataString(query)}&lang=en&max={MaxArticles}&apikey={{0}}",
            cancellationToken);

    private async Task<IReadOnlyList<GNewsArticleDto>> FetchAsync(string relativeUrlTemplate, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            _logger.LogWarning("GNews:ApiKey is not configured - returning no articles.");
            return [];
        }

        var relativeUrl = string.Format(relativeUrlTemplate, _options.ApiKey);

        try
        {
            using var response = await _httpClient.GetAsync(relativeUrl, cancellationToken);
            var body = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                // Covers GNews's rate-limit (403/429) and error responses alike - logged with enough
                // of the body to diagnose, never thrown, per this class's doc comment.
                _logger.LogWarning(
                    "GNews request failed with status {StatusCode}: {Body}",
                    response.StatusCode,
                    Truncate(body, 500));
                return [];
            }

            var payload = JsonSerializer.Deserialize<GNewsResponseDto>(body, JsonOptions);
            if (payload?.Articles is null)
            {
                return [];
            }

            return payload.Articles
                .Where(a => !string.IsNullOrWhiteSpace(a.Id) && !string.IsNullOrWhiteSpace(a.Title) && !string.IsNullOrWhiteSpace(a.Url))
                .Select(a => new GNewsArticleDto(
                    a.Id!,
                    a.Title!,
                    a.Description,
                    a.Content,
                    a.Url!,
                    a.Image,
                    a.Source?.Name ?? "Unknown",
                    a.PublishedAt ?? DateTime.UtcNow))
                .ToList();
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or JsonException)
        {
            _logger.LogWarning(ex, "GNews request threw an exception - returning no articles.");
            return [];
        }
    }

    private static string Truncate(string text, int maxLength) => text.Length <= maxLength ? text : text[..maxLength];

    // Plain (non-record) classes with nullable properties, deserialized with JsonSerializerDefaults.Web
    // (case-insensitive camelCase matching) - same tolerant-raw-DTO shape NoteAiResponseMapper and
    // StudyPlanAiResponseParser use for their own external/AI JSON payloads.
    private sealed class GNewsResponseDto
    {
        [JsonPropertyName("totalArticles")]
        public int TotalArticles { get; set; }

        public List<GNewsArticleRawDto>? Articles { get; set; }
    }

    private sealed class GNewsArticleRawDto
    {
        public string? Id { get; set; }

        public string? Title { get; set; }

        public string? Description { get; set; }

        public string? Content { get; set; }

        public string? Url { get; set; }

        public string? Image { get; set; }

        public DateTime? PublishedAt { get; set; }

        public string? Lang { get; set; }

        public GNewsSourceRawDto? Source { get; set; }
    }

    private sealed class GNewsSourceRawDto
    {
        public string? Id { get; set; }

        public string? Name { get; set; }

        public string? Url { get; set; }
    }
}
