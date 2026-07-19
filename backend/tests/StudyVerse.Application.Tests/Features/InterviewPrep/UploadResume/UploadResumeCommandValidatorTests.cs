using FluentAssertions;
using StudyVerse.Application.Features.InterviewPrep.Common;
using StudyVerse.Application.Features.InterviewPrep.UploadResume;

namespace StudyVerse.Application.Tests.Features.InterviewPrep.UploadResume;

public sealed class UploadResumeCommandValidatorTests
{
    private readonly UploadResumeCommandValidator _validator = new();

    private static UploadResumeCommand Command(string fileName, long fileSizeBytes = 1024, string contentType = "application/octet-stream") =>
        new(Guid.NewGuid(), Stream.Null, fileName, contentType, fileSizeBytes);

    [Theory]
    [InlineData("resume.pdf")]
    [InlineData("resume.docx")]
    [InlineData("RESUME.PDF")]
    public void Validate_SupportedFileTypes_PassesFileTypeCheck(string fileName)
    {
        var result = _validator.Validate(Command(fileName));

        result.Errors.Should().NotContain(e => e.PropertyName == nameof(UploadResumeCommand.FileName) && e.ErrorMessage.Contains("Unsupported file type"));
    }

    [Theory]
    [InlineData("resume.jpg")]
    [InlineData("resume.png")]
    [InlineData("resume.txt")]
    [InlineData("resume.pptx")]
    [InlineData("resume.zip")]
    [InlineData("no-extension")]
    public void Validate_UnsupportedFileTypes_FailsWithClearMessage(string fileName)
    {
        var result = _validator.Validate(Command(fileName));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage.Contains("Unsupported file type"));
    }

    [Fact]
    public void Validate_FileOverTenMegabytes_FailsWithSizeLimitMessage()
    {
        var oversized = ResumeFileTypeResolver.MaxFileSizeBytes + 1;

        var result = _validator.Validate(Command("resume.pdf", fileSizeBytes: oversized));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage.Contains("10MB") || e.ErrorMessage.Contains("smaller"));
    }

    [Fact]
    public void Validate_EmptyFile_FailsWithEmptyFileMessage()
    {
        var result = _validator.Validate(Command("resume.pdf", fileSizeBytes: 0));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage.Contains("empty"));
    }

    [Fact]
    public void Validate_MissingUserId_Fails()
    {
        var command = Command("resume.pdf") with { UserId = Guid.Empty };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UploadResumeCommand.UserId));
    }
}
