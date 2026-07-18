using FluentValidation;

namespace StudyVerse.Application.Features.Flashcards.GenerateDeckFromNote;

public sealed class GenerateDeckFromNoteCommandValidator : AbstractValidator<GenerateDeckFromNoteCommand>
{
    public GenerateDeckFromNoteCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.NoteId).NotEmpty();
    }
}
