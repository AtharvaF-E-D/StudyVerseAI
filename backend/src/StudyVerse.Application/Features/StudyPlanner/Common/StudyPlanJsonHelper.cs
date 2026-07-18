using System.Text.Json;

namespace StudyVerse.Application.Features.StudyPlanner.Common;

/// <summary>
/// Serializes/deserializes the small string lists (<c>Subjects</c>, <c>WeakTopics</c>) that
/// <see cref="StudyVerse.Domain.Entities.StudyPlan"/> stores as JSON text columns — see that
/// entity's doc comment for why a JSON column beats a child table here. Deliberately tolerant like
/// <c>NoteAiResponseMapper</c>: a missing/malformed column defaults to an empty list rather than
/// throwing, since these lists are always fully replaced as a whole, never that important that a
/// read should fail outright.
/// </summary>
internal static class StudyPlanJsonHelper
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public static string Serialize(IReadOnlyList<string> values) =>
        JsonSerializer.Serialize(values, JsonOptions);

    public static IReadOnlyList<string> Deserialize(string json)
    {
        try
        {
            return JsonSerializer.Deserialize<List<string>>(json, JsonOptions) ?? [];
        }
        catch (JsonException)
        {
            return [];
        }
    }
}
