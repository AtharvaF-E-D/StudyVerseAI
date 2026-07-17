using FluentValidation;

namespace StudyVerse.Application.Features.Notes.GetNotes;

public sealed class GetNotesQueryValidator : AbstractValidator<GetNotesQuery>
{
    public GetNotesQueryValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.Take).GreaterThan(0);
    }
}
