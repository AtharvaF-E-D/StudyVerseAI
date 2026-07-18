using System.Text.Json;
using MediatR;
using Microsoft.EntityFrameworkCore;
using StudyVerse.Application.Common.Interfaces;
using StudyVerse.Application.Features.CurrentAffairs.Common;
using StudyVerse.Domain.Common;
using StudyVerse.Domain.Entities;
using StudyVerse.Domain.Enums;

namespace StudyVerse.Application.Features.CurrentAffairs.GetArticleQuiz;

/// <summary>
/// If a <see cref="NewsArticleQuiz"/> already exists for this article, returns it straight from the
/// DB - no <see cref="IAiChatProvider"/> call. Otherwise builds a one-off prompt from the article's
/// actual title/content (falling back to its description when GNews's free-tier content field is
/// empty/truncated), calls <see cref="IAiChatProvider.GetCompletionAsync"/> in JSON mode (reusing the
/// tutor's chat provider exactly like <c>OpenAiNoteGenerationProvider</c>/mock-test weakness analysis
/// do - no new AI abstraction for this feature per the phase spec), parses the response the same
/// defensively-tolerant way <c>NoteAiResponseMapper</c>/<c>StudyPlanAiResponseParser</c> do, persists
/// it, and returns it. Every later call for the same article hits the cache-first branch above.
/// </summary>
public sealed class GetArticleQuizQueryHandler : IRequestHandler<GetArticleQuizQuery, Result<NewsArticleQuizDto>>
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly IAppDbContext _db;
    private readonly IAiChatProvider _aiChatProvider;
    private readonly IDateTimeProvider _dateTimeProvider;

    public GetArticleQuizQueryHandler(IAppDbContext db, IAiChatProvider aiChatProvider, IDateTimeProvider dateTimeProvider)
    {
        _db = db;
        _aiChatProvider = aiChatProvider;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<Result<NewsArticleQuizDto>> Handle(GetArticleQuizQuery request, CancellationToken cancellationToken)
    {
        var article = await _db.NewsArticles.FirstOrDefaultAsync(a => a.Id == request.ArticleId, cancellationToken);
        if (article is null)
        {
            return Result.Failure<NewsArticleQuizDto>("Article not found.", ResultErrorType.NotFound);
        }

        var existingQuiz = await _db.NewsArticleQuizzes.FirstOrDefaultAsync(q => q.ArticleId == article.Id, cancellationToken);
        if (existingQuiz is not null)
        {
            return Result.Success(ToDto(existingQuiz));
        }

        var articleText = string.IsNullOrWhiteSpace(article.Content) ? article.Description ?? string.Empty : article.Content;
        var prompt = NewsQuizPromptBuilder.Build(article.Title, articleText);

        var completion = await _aiChatProvider.GetCompletionAsync(
            [new AiChatMessage(MessageRole.User, prompt)],
            cancellationToken,
            requireJsonObjectResponse: true);

        var questions = NewsArticleQuizResponseParser.Parse(completion.Content);
        if (questions.Count == 0)
        {
            return Result.Failure<NewsArticleQuizDto>("The AI didn't return a usable quiz for this article. Please try again.");
        }

        var quiz = new NewsArticleQuiz
        {
            Id = Guid.NewGuid(),
            ArticleId = article.Id,
            QuestionsJson = JsonSerializer.Serialize(questions, JsonOptions),
            GeneratedAtUtc = _dateTimeProvider.UtcNow,
        };
        _db.NewsArticleQuizzes.Add(quiz);
        await _db.SaveChangesAsync(cancellationToken);

        return Result.Success(ToDto(quiz));
    }

    private static NewsArticleQuizDto ToDto(NewsArticleQuiz quiz)
    {
        var questions = JsonSerializer.Deserialize<List<GeneratedQuizQuestion>>(quiz.QuestionsJson, JsonOptions) ?? [];

        return new NewsArticleQuizDto(
            quiz.ArticleId,
            questions
                .Select(q => new NewsArticleQuizQuestionDto(q.QuestionText, q.Options, q.CorrectOptionIndex, q.Explanation))
                .ToList(),
            quiz.GeneratedAtUtc);
    }
}
