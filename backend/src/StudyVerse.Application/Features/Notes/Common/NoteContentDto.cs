namespace StudyVerse.Application.Features.Notes.Common;

/// <summary>All seven pieces of a note's AI-generated content, in their fully structured (not
/// JSON-text) form — what <c>NoteAiResponseMapper</c> parses the raw AI response into, and what
/// <c>GetNoteQueryHandler</c> deserializes a persisted <see cref="Domain.Entities.NoteContent"/>
/// back into for the API response.</summary>
public sealed record NoteContentDto(
    string Summary,
    IReadOnlyList<string> KeyPoints,
    IReadOnlyList<FlashcardDto> Flashcards,
    IReadOnlyList<McqDto> Mcqs,
    MindMapNodeDto MindMap,
    string RevisionSheet,
    IReadOnlyList<VocabularyTermDto> Vocabulary,
    IReadOnlyList<FormulaDto> Formulas);

public sealed record FlashcardDto(string Question, string Answer);

public sealed record McqDto(string Question, IReadOnlyList<string> Options, int CorrectOptionIndex, string Explanation);

/// <summary>One node of the mind map outline tree — see <see cref="Domain.Entities.NoteContent"/>'s
/// doc comment for why this is an indented outline rather than a visual canvas.</summary>
public sealed record MindMapNodeDto(string Topic, IReadOnlyList<MindMapNodeDto> Children);

public sealed record VocabularyTermDto(string Term, string Definition);

public sealed record FormulaDto(string Name, string Formula, string Explanation);
