using FluentValidation;

namespace StudyVerse.Application.Features.Notes.GetNote;

public sealed class GetNoteQueryValidator : AbstractValidator<GetNoteQuery>
{
    public GetNoteQueryValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.NoteId).NotEmpty();
    }
}
