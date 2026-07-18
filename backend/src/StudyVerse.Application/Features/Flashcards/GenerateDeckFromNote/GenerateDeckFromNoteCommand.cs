using MediatR;
using StudyVerse.Domain.Common;

namespace StudyVerse.Application.Features.Flashcards.GenerateDeckFromNote;

/// <summary>Copies an existing, already-<see cref="StudyVerse.Domain.Enums.NoteStatus.Ready"/>
/// note's AI-generated flashcards (Phase 6) into a brand-new deck — no extra OpenAI call needed,
/// the content already exists.</summary>
public sealed record GenerateDeckFromNoteCommand(Guid UserId, Guid NoteId) : IRequest<Result<Guid>>;
