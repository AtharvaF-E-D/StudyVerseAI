namespace StudyVerse.Domain.Enums;

/// <summary>
/// Lifecycle of a <see cref="Entities.Note"/>'s AI-generated content. There is no background job
/// queue for this pass (Phase 16 territory) — a note is created as <see cref="Processing"/> and
/// moves synchronously, within the same upload request, to either <see cref="Ready"/> or
/// <see cref="Failed"/>. It should never be left stuck at <see cref="Processing"/>.
/// </summary>
public enum NoteStatus
{
    Processing = 0,
    Ready = 1,
    Failed = 2,
}
