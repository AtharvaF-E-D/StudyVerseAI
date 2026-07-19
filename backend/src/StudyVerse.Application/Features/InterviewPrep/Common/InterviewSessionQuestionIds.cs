using System.Text.Json;

namespace StudyVerse.Application.Features.InterviewPrep.Common;

/// <summary>
/// Serializes/deserializes <see cref="StudyVerse.Domain.Entities.InterviewSession.SelectedQuestionIdsJson"/> —
/// a plain JSON array of <see cref="Guid"/>, in the fixed order the questions were selected in.
/// </summary>
internal static class InterviewSessionQuestionIds
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public static string Serialize(IEnumerable<Guid> ids) => JsonSerializer.Serialize(ids, JsonOptions);

    public static IReadOnlyList<Guid> Deserialize(string json)
    {
        try
        {
            return JsonSerializer.Deserialize<List<Guid>>(json, JsonOptions) ?? [];
        }
        catch (JsonException)
        {
            return [];
        }
    }
}
