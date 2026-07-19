using FluentValidation;
using StudyVerse.Application.Features.InterviewPrep.Common;

namespace StudyVerse.Application.Features.InterviewPrep.UploadResume;

public sealed class UploadResumeCommandValidator : AbstractValidator<UploadResumeCommand>
{
    public UploadResumeCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();

        RuleFor(x => x.FileName)
            .NotEmpty().WithMessage("A file name is required.")
            .MaximumLength(260);

        RuleFor(x => x.FileSizeBytes)
            .GreaterThan(0).WithMessage("The uploaded file is empty.")
            .LessThanOrEqualTo(ResumeFileTypeResolver.MaxFileSizeBytes)
            .WithMessage($"Files must be {ResumeFileTypeResolver.MaxFileSizeBytes / (1024 * 1024)}MB or smaller.");

        RuleFor(x => x.FileName)
            .Must(fileName => ResumeFileTypeResolver.TryResolve(fileName, out _))
            .WithMessage("Unsupported file type. Only PDF and DOCX files are supported.")
            .When(x => !string.IsNullOrEmpty(x.FileName));
    }
}
