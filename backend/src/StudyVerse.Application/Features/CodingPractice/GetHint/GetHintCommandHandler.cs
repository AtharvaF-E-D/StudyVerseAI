using MediatR;
using Microsoft.EntityFrameworkCore;
using StudyVerse.Application.Common.Interfaces;
using StudyVerse.Application.Features.CodingPractice.Common;
using StudyVerse.Domain.Common;
using StudyVerse.Domain.Enums;

namespace StudyVerse.Application.Features.CodingPractice.GetHint;

/// <summary>
/// One-off AI hint for whatever code the student currently has - no caching (unlike the Notes/quiz
/// AI features, a hint is contextual to the student's current, possibly-changing code, so there's
/// nothing stable to cache; see <see cref="CodingHintPromptBuilder"/>'s doc comment). Reuses the
/// existing <see cref="IAiChatProvider"/> exactly like <c>GetArticleQuizQueryHandler</c> does - no
/// new AI abstraction for this feature, per the phase spec.
/// </summary>
public sealed class GetHintCommandHandler : IRequestHandler<GetHintCommand, Result<CodingHintDto>>
{
    private readonly IAppDbContext _db;
    private readonly IAiChatProvider _aiChatProvider;

    public GetHintCommandHandler(IAppDbContext db, IAiChatProvider aiChatProvider)
    {
        _db = db;
        _aiChatProvider = aiChatProvider;
    }

    public async Task<Result<CodingHintDto>> Handle(GetHintCommand request, CancellationToken cancellationToken)
    {
        var problem = await _db.CodingProblems.FirstOrDefaultAsync(p => p.Id == request.ProblemId, cancellationToken);
        if (problem is null)
        {
            return Result.Failure<CodingHintDto>("Problem not found.", ResultErrorType.NotFound);
        }

        var prompt = CodingHintPromptBuilder.Build(problem.Title, problem.Description, request.CurrentCode);

        var completion = await _aiChatProvider.GetCompletionAsync(
            [new AiChatMessage(MessageRole.User, prompt)],
            cancellationToken);

        return Result.Success(new CodingHintDto(completion.Content.Trim()));
    }
}
