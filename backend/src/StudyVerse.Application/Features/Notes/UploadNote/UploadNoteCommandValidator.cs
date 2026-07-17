using FluentValidation;
using StudyVerse.Application.Features.Notes.Common;

namespace StudyVerse.Application.Features.Notes.UploadNote;

public sealed class UploadNoteCommandValidator : AbstractValidator<UploadNoteCommand>
{
    public UploadNoteCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();

        RuleFor(x => x.FileName)
            .NotEmpty().WithMessage("A file name is required.")
            .MaximumLength(260);

        RuleFor(x => x.FileSizeBytes)
            .GreaterThan(0).WithMessage("The uploaded file is empty.")
            .LessThanOrEqualTo(NoteFileTypeResolver.MaxFileSizeBytes)
            .WithMessage($"Files must be {NoteFileTypeResolver.MaxFileSizeBytes / (1024 * 1024)}MB or smaller.");

        RuleFor(x => x.FileName)
            .Must(fileName => NoteFileTypeResolver.TryResolve(fileName, out _))
            .WithMessage("Unsupported file type. Only PDF, DOCX, JPG, and PNG files are supported.")
            .When(x => !string.IsNullOrEmpty(x.FileName));
    }
}
