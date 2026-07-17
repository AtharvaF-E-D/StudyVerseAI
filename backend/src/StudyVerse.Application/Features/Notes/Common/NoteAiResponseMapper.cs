using System.Text.Json;
using StudyVerse.Domain.Common;
using StudyVerse.Domain.Entities;

namespace StudyVerse.Application.Features.Notes.Common;

/// <summary>
/// Parses the raw JSON returned by <c>INoteGenerationProvider.GenerateNoteContentJsonAsync</c> into
/// a <see cref="NoteContentDto"/>, and converts between that DTO and the persisted
/// <see cref="NoteContent"/> entity (whose seven pieces live as JSON text columns — see that
/// entity's doc comment for why).
///
/// Deliberately tolerant of the model omitting a field it considered not applicable (e.g. no
/// formulas for a history note) or the JSON using slightly different casing — those default to an
/// empty collection/string rather than throwing. The one thing that IS a genuine failure — and the
/// only thing <see cref="Parse"/> returns a failed <see cref="Result{T}"/> for — is the top-level
/// response not being parseable as a JSON object at all, which the caller (UploadNoteCommandHandler)
/// turns into <c>Note.Status = Failed</c> rather than leaving the note stuck at Processing.
/// </summary>
public static class NoteAiResponseMapper
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public static Result<NoteContentDto> Parse(string rawJson)
    {
        RawNoteAiResponse? raw;
        try
        {
            raw = JsonSerializer.Deserialize<RawNoteAiResponse>(rawJson, JsonOptions);
        }
        catch (JsonException ex)
        {
            return Result.Failure<NoteContentDto>($"Could not parse the AI note response as JSON: {ex.Message}");
        }

        if (raw is null)
        {
            return Result.Failure<NoteContentDto>("The AI note response was empty.");
        }

        var dto = new NoteContentDto(
            raw.Summary ?? string.Empty,
            raw.KeyPoints ?? [],
            (raw.Flashcards ?? [])
                .Select(f => new FlashcardDto(f.Question ?? string.Empty, f.Answer ?? string.Empty))
                .ToList(),
            (raw.Mcqs ?? [])
                .Select(m => new McqDto(
                    m.Question ?? string.Empty,
                    NormalizeOptions(m.Options),
                    Math.Clamp(m.CorrectOptionIndex, 0, 3),
                    m.Explanation ?? string.Empty))
                .ToList(),
            raw.MindMap is not null ? MapMindMapNode(raw.MindMap) : new MindMapNodeDto("Untitled", []),
            raw.RevisionSheet ?? string.Empty,
            (raw.Vocabulary ?? [])
                .Select(v => new VocabularyTermDto(v.Term ?? string.Empty, v.Definition ?? string.Empty))
                .ToList(),
            (raw.Formulas ?? [])
                .Select(f => new FormulaDto(f.Name ?? string.Empty, f.Formula ?? string.Empty, f.Explanation ?? string.Empty))
                .ToList());

        return Result.Success(dto);
    }

    public static NoteContent ToEntity(Guid noteId, NoteContentDto dto) => new()
    {
        Id = Guid.NewGuid(),
        NoteId = noteId,
        Summary = dto.Summary,
        KeyPointsJson = JsonSerializer.Serialize(dto.KeyPoints, JsonOptions),
        FlashcardsJson = JsonSerializer.Serialize(dto.Flashcards, JsonOptions),
        McqsJson = JsonSerializer.Serialize(dto.Mcqs, JsonOptions),
        MindMapJson = JsonSerializer.Serialize(dto.MindMap, JsonOptions),
        RevisionSheet = dto.RevisionSheet,
        VocabularyJson = JsonSerializer.Serialize(dto.Vocabulary, JsonOptions),
        FormulasJson = JsonSerializer.Serialize(dto.Formulas, JsonOptions),
    };

    public static NoteContentDto FromEntity(NoteContent entity) => new(
        entity.Summary,
        JsonSerializer.Deserialize<List<string>>(entity.KeyPointsJson, JsonOptions) ?? [],
        JsonSerializer.Deserialize<List<FlashcardDto>>(entity.FlashcardsJson, JsonOptions) ?? [],
        JsonSerializer.Deserialize<List<McqDto>>(entity.McqsJson, JsonOptions) ?? [],
        JsonSerializer.Deserialize<MindMapNodeDto>(entity.MindMapJson, JsonOptions) ?? new MindMapNodeDto("Untitled", []),
        entity.RevisionSheet,
        JsonSerializer.Deserialize<List<VocabularyTermDto>>(entity.VocabularyJson, JsonOptions) ?? [],
        JsonSerializer.Deserialize<List<FormulaDto>>(entity.FormulasJson, JsonOptions) ?? []);

    /// <summary>Pads/truncates to exactly 4 options so the client can always render A-D — a model
    /// that deviates from the requested schema shouldn't crash rendering for the whole note.</summary>
    private static IReadOnlyList<string> NormalizeOptions(List<string>? options)
    {
        var list = options ?? [];
        if (list.Count == 4)
        {
            return list;
        }

        var normalized = new List<string>(list.Take(4));
        while (normalized.Count < 4)
        {
            normalized.Add(string.Empty);
        }

        return normalized;
    }

    private static MindMapNodeDto MapMindMapNode(RawMindMapNode node) => new(
        node.Topic ?? string.Empty,
        (node.Children ?? []).Select(MapMindMapNode).ToList());

    // Plain (non-record) classes with nullable properties: lets deserialization leave a missing
    // field as null (detected and defaulted above) instead of System.Text.Json throwing for a
    // record's required positional constructor parameter.
    private sealed class RawNoteAiResponse
    {
        public string? Summary { get; set; }

        public List<string>? KeyPoints { get; set; }

        public List<RawFlashcard>? Flashcards { get; set; }

        public List<RawMcq>? Mcqs { get; set; }

        public RawMindMapNode? MindMap { get; set; }

        public string? RevisionSheet { get; set; }

        public List<RawVocabularyTerm>? Vocabulary { get; set; }

        public List<RawFormula>? Formulas { get; set; }
    }

    private sealed class RawFlashcard
    {
        public string? Question { get; set; }

        public string? Answer { get; set; }
    }

    private sealed class RawMcq
    {
        public string? Question { get; set; }

        public List<string>? Options { get; set; }

        public int CorrectOptionIndex { get; set; }

        public string? Explanation { get; set; }
    }

    private sealed class RawMindMapNode
    {
        public string? Topic { get; set; }

        public List<RawMindMapNode>? Children { get; set; }
    }

    private sealed class RawVocabularyTerm
    {
        public string? Term { get; set; }

        public string? Definition { get; set; }
    }

    private sealed class RawFormula
    {
        public string? Name { get; set; }

        public string? Formula { get; set; }

        public string? Explanation { get; set; }
    }
}
