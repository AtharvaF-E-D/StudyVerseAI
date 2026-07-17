using FluentValidation;

namespace StudyVerse.Application.Features.Notes.DeleteNote;

public sealed class DeleteNoteCommandValidator : AbstractValidator<DeleteNoteCommand>
{
    public DeleteNoteCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.NoteId).NotEmpty();
    }
}
