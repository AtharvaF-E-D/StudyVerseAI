using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StudyVerse.Application.Common.Interfaces;
using StudyVerse.Infrastructure.Options;

namespace StudyVerse.Infrastructure.External;

/// <summary>
/// Calls the real Judge0 CE REST API via RapidAPI (https://judge0-ce.p.rapidapi.com/) using a plain
/// <see cref="HttpClient"/> - same shape as <see cref="GNewsProvider"/> (a plain REST endpoint with
/// no official SDK), registered the same way via <c>AddHttpClient(nameof(Judge0Provider), ...)</c>
/// in <c>DependencyInjection</c>, with the base address and the two fixed RapidAPI headers
/// (<c>x-rapidapi-host</c> is a constant; <c>x-rapidapi-key</c> is set per-request below since it
/// comes from configuration, not a compile-time constant) set there/here respectively.
///
/// Uses <c>?base64_encoded=false&amp;wait=true</c> and passes <c>expected_output</c> in the request
/// body, so Judge0 itself does the stdout comparison synchronously in one round trip - no polling,
/// no base64 encode/decode. RapidAPI's free tier has a real daily request quota; any failure
/// (network error, non-2xx response - including a 429 rate-limit - or an unparseable body) is
/// logged (including RapidAPI's rate-limit response headers when present, to make quota exhaustion
/// diagnosable) and swallowed, returning the <c>"Error"</c> sentinel status rather than throwing -
/// see <see cref="IJudge0Provider"/>'s doc comment for why.
/// </summary>
public sealed class Judge0Provider : IJudge0Provider
{
    private const string RateLimitRemainingHeader = "X-RateLimit-Requests-Remaining";
    private const string RateLimitLimitHeader = "X-RateLimit-Requests-Limit";

    private readonly HttpClient _httpClient;
    private readonly Judge0Options _options;
    private readonly ILogger<Judge0Provider> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public Judge0Provider(IHttpClientFactory httpClientFactory, IOptions<Judge0Options> options, ILogger<Judge0Provider> logger)
    {
        _httpClient = httpClientFactory.CreateClient(nameof(Judge0Provider));
        _options = options.Value;
        _logger = logger;
    }

    public async Task<Judge0ResultDto> RunAsync(
        int languageId,
        string sourceCode,
        string stdin,
        string expectedOutput,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_options.RapidApiKey))
        {
            _logger.LogWarning("Judge0:RapidApiKey is not configured - returning an Error status.");
            return new Judge0ResultDto("Error", null, null, null);
        }

        var requestBody = JsonSerializer.Serialize(
            new Judge0SubmissionRequest(languageId, sourceCode, stdin, expectedOutput),
            JsonOptions);

        using var request = new HttpRequestMessage(HttpMethod.Post, "submissions?base64_encoded=false&wait=true")
        {
            Content = new StringContent(requestBody, Encoding.UTF8, "application/json"),
        };
        request.Headers.Add("x-rapidapi-key", _options.RapidApiKey);

        try
        {
            using var response = await _httpClient.SendAsync(request, cancellationToken);
            var body = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var remaining = response.Headers.TryGetValues(RateLimitRemainingHeader, out var remainingValues)
                    ? string.Join(",", remainingValues)
                    : "unknown";
                var limit = response.Headers.TryGetValues(RateLimitLimitHeader, out var limitValues)
                    ? string.Join(",", limitValues)
                    : "unknown";

                _logger.LogWarning(
                    "Judge0 request failed with status {StatusCode} (rate-limit remaining {Remaining}/{Limit}): {Body}",
                    response.StatusCode,
                    remaining,
                    limit,
                    Truncate(body, 500));
                return new Judge0ResultDto("Error", null, null, null);
            }

            var payload = JsonSerializer.Deserialize<Judge0SubmissionResponse>(body, JsonOptions);
            if (payload?.Status?.Description is null)
            {
                _logger.LogWarning("Judge0 returned an unparseable response: {Body}", Truncate(body, 500));
                return new Judge0ResultDto("Error", null, null, null);
            }

            return new Judge0ResultDto(payload.Status.Description, payload.Stdout, payload.Stderr, payload.CompileOutput);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or JsonException)
        {
            _logger.LogWarning(ex, "Judge0 request threw an exception - returning an Error status.");
            return new Judge0ResultDto("Error", null, null, null);
        }
    }

    private static string Truncate(string text, int maxLength) => text.Length <= maxLength ? text : text[..maxLength];

    private sealed record Judge0SubmissionRequest(
        [property: JsonPropertyName("language_id")] int LanguageId,
        [property: JsonPropertyName("source_code")] string SourceCode,
        [property: JsonPropertyName("stdin")] string Stdin,
        [property: JsonPropertyName("expected_output")] string ExpectedOutput);

    // Plain (non-record) classes with nullable properties, deserialized with JsonSerializerDefaults.Web
    // (case-insensitive camelCase matching) - same tolerant-raw-DTO shape GNewsProvider uses for its
    // own external JSON payload.
    private sealed class Judge0SubmissionResponse
    {
        public string? Stdout { get; set; }

        public string? Stderr { get; set; }

        [JsonPropertyName("compile_output")]
        public string? CompileOutput { get; set; }

        public string? Message { get; set; }

        public Judge0StatusDto? Status { get; set; }
    }

    private sealed class Judge0StatusDto
    {
        public int Id { get; set; }

        public string? Description { get; set; }
    }
}
