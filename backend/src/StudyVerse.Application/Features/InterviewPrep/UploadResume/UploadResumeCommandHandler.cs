using System.Text.Json;
using MediatR;
using StudyVerse.Application.Common.Interfaces;
using StudyVerse.Application.Features.InterviewPrep.Common;
using StudyVerse.Domain.Common;
using StudyVerse.Domain.Entities;
using StudyVerse.Domain.Enums;

namespace StudyVerse.Application.Features.InterviewPrep.UploadResume;

/// <summary>
/// Reuses the exact Phase 6 file-upload/text-extraction pipeline
/// (<see cref="IFileStorageService"/>/<see cref="ITextExtractionService"/>) that
/// <c>UploadNoteCommandHandler</c> uses, restricted to PDF/DOCX (see
/// <see cref="ResumeFileTypeResolver"/> — no image OCR needed for resumes), then makes one
/// <see cref="IAiChatProvider"/> JSON-mode call analyzing the extracted resume text. Unlike Notes
/// (which persists a <c>Processing</c> row before the slow AI step so a client always has an id to
/// retry against — see that handler's doc comment), a resume analysis has no useful intermediate
/// state to expose: if extraction or the AI call fails, nothing is persisted and the whole request
/// fails, so the caller can safely just retry the upload.
/// </summary>
public sealed class UploadResumeCommandHandler : IRequestHandler<UploadResumeCommand, Result<ResumeAnalysisDto>>
{
    private readonly IAppDbContext _db;
    private readonly IFileStorageService _fileStorage;
    private readonly ITextExtractionService _textExtraction;
    private readonly IAiChatProvider _aiChatProvider;
    private readonly IDateTimeProvider _dateTimeProvider;

    public UploadResumeCommandHandler(
        IAppDbContext db,
        IFileStorageService fileStorage,
        ITextExtractionService textExtraction,
        IAiChatProvider aiChatProvider,
        IDateTimeProvider dateTimeProvider)
    {
        _db = db;
        _fileStorage = fileStorage;
        _textExtraction = textExtraction;
        _aiChatProvider = aiChatProvider;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<Result<ResumeAnalysisDto>> Handle(UploadResumeCommand request, CancellationToken cancellationToken)
    {
        // Already checked by UploadResumeCommandValidator, re-checked here because it determines
        // which extraction strategy runs and isn't something we can proceed without.
        if (!ResumeFileTypeResolver.TryResolve(request.FileName, out var fileType))
        {
            return Result.Failure<ResumeAnalysisDto>("Unsupported file type. Only PDF and DOCX files are supported.");
        }

        var storageKey = await _fileStorage.SaveAsync(request.FileStream, request.FileName, cancellationToken);

        // mediaType is only ever consulted for NoteSourceFileType.Image (see ITextExtractionService's
        // doc comment) - unreachable here since ResumeFileTypeResolver only ever resolves Pdf/Docx.
        await using var readStream = await _fileStorage.OpenReadAsync(storageKey, cancellationToken);
        var extractedText = await _textExtraction.ExtractTextAsync(readStream, fileType, "application/octet-stream", cancellationToken);

        if (string.IsNullOrWhiteSpace(extractedText))
        {
            return Result.Failure<ResumeAnalysisDto>("No text could be extracted from the uploaded resume.");
        }

        var prompt = ResumeAnalysisPromptBuilder.Build(extractedText);
        var completion = await _aiChatProvider.GetCompletionAsync(
            [new AiChatMessage(MessageRole.User, prompt)],
            cancellationToken,
            requireJsonObjectResponse: true);

        var parsed = ResumeAnalysisResponseParser.Parse(completion.Content);
        if (parsed is null)
        {
            return Result.Failure<ResumeAnalysisDto>("The AI didn't return a usable analysis for this resume. Please try again.");
        }

        var now = _dateTimeProvider.UtcNow;
        var analysis = new ResumeAnalysis
        {
            Id = Guid.NewGuid(),
            UserId = request.UserId,
            FileName = request.FileName,
            StoredFilePath = storageKey,
            OverallScore = parsed.OverallScore,
            StrengthsJson = JsonSerializer.Serialize(parsed.Strengths),
            WeaknessesJson = JsonSerializer.Serialize(parsed.Weaknesses),
            SuggestionsJson = JsonSerializer.Serialize(parsed.Suggestions),
            AnalyzedAtUtc = now,
        };
        _db.ResumeAnalyses.Add(analysis);
        await _db.SaveChangesAsync(cancellationToken);

        var dto = new ResumeAnalysisDto(
            analysis.Id,
            analysis.FileName,
            analysis.OverallScore,
            parsed.Strengths,
            parsed.Weaknesses,
            parsed.Suggestions,
            analysis.AnalyzedAtUtc);

        return Result.Success(dto);
    }
}
