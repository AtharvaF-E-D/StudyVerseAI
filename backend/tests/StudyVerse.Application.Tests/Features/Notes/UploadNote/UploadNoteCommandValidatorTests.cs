using FluentAssertions;
using StudyVerse.Application.Features.Notes.Common;
using StudyVerse.Application.Features.Notes.UploadNote;

namespace StudyVerse.Application.Tests.Features.Notes.UploadNote;

public sealed class UploadNoteCommandValidatorTests
{
    private readonly UploadNoteCommandValidator _validator = new();

    private static UploadNoteCommand Command(string fileName, long fileSizeBytes = 1024, string contentType = "application/octet-stream") =>
        new(Guid.NewGuid(), Stream.Null, fileName, contentType, fileSizeBytes);

    [Theory]
    [InlineData("lecture-notes.pdf")]
    [InlineData("essay.docx")]
    [InlineData("scan.jpg")]
    [InlineData("scan.jpeg")]
    [InlineData("scan.png")]
    [InlineData("SCAN.PNG")]
    public void Validate_SupportedFileTypes_PassesFileTypeCheck(string fileName)
    {
        var result = _validator.Validate(Command(fileName));

        result.Errors.Should().NotContain(e => e.PropertyName == nameof(UploadNoteCommand.FileName) && e.ErrorMessage.Contains("Unsupported file type"));
    }

    [Theory]
    [InlineData("archive.zip")]
    [InlineData("presentation.pptx")]
    [InlineData("script.exe")]
    [InlineData("notes.txt")]
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
        var oversized = NoteFileTypeResolver.MaxFileSizeBytes + 1;

        var result = _validator.Validate(Command("thesis.pdf", fileSizeBytes: oversized));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage.Contains("10MB") || e.ErrorMessage.Contains("smaller"));
    }

    [Fact]
    public void Validate_FileAtExactlyTenMegabytes_Passes()
    {
        var result = _validator.Validate(Command("thesis.pdf", fileSizeBytes: NoteFileTypeResolver.MaxFileSizeBytes));

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_EmptyFile_FailsWithEmptyFileMessage()
    {
        var result = _validator.Validate(Command("notes.pdf", fileSizeBytes: 0));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage.Contains("empty"));
    }

    [Fact]
    public void Validate_MissingUserId_Fails()
    {
        var command = Command("notes.pdf") with { UserId = Guid.Empty };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UploadNoteCommand.UserId));
    }
}
